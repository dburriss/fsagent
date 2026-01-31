namespace FsAgent.Writers

open System
open System.Text
open System.Text.Json
open YamlDotNet.Serialization
open FsAgent.AST
open FsAgent.Prompts
open FsAgent.Agents

module Template =
    open Fue.Data
    open Fue.Compiler

    type TemplateVariables = Map<string, obj>

    let renderInline (text: string) (variables: TemplateVariables) : string =
        try
            let data =
                variables
                |> Map.fold (fun acc key value -> acc |> add key value) init
            data |> fromText text
        with
        | ex -> $"[Template error: {ex.Message}]"

    let renderFile (path: string) (variables: TemplateVariables) : string =
        try
            if not (System.IO.File.Exists(path)) then
                $"[Template file not found: {path}]"
            else
                let data =
                    variables
                    |> Map.fold (fun acc key value -> acc |> add key value) init
                data |> fromFile path
        with
        | ex -> $"[Template error: {ex.Message}]"

module MarkdownWriter =

    type AgentFormat =
        | Opencode
        | Copilot

    type OutputType =
        | Md
        | Json
        | Yaml

    type WriterContext = {
        Format: AgentFormat
        OutputType: OutputType
        Timestamp: DateTime
        AgentName: string option
        AgentDescription: string option
    }

    type Options = {
        mutable OutputFormat: AgentFormat
        mutable OutputType: OutputType
        mutable DisableCodeBlockWrapping: bool
        mutable RenameMap: Map<string, string>
        mutable HeadingFormatter: (string -> string) option
        mutable GeneratedFooter: (WriterContext -> string) option
        mutable IncludeFrontmatter: bool
        mutable CustomWriter: (Agent -> Options -> string) option
        mutable TemplateVariables: Template.TemplateVariables
    }

    let formatToLanguageTag (format: DataFormat) : string =
        match format with
        | DataFormat.Yaml -> "yaml"
        | DataFormat.Json -> "json"
        | DataFormat.Toon -> "toon"
        | DataFormat.Unknown -> ""

    let defaultOptions () = {
        OutputFormat = Opencode
        OutputType = Md
        DisableCodeBlockWrapping = false
        RenameMap = Map.empty
        HeadingFormatter = None
        GeneratedFooter = None
        IncludeFrontmatter = true
        CustomWriter = None
        TemplateVariables = Map.empty
    }

    let rec nodeToObj (node: Node) (templateVars: Template.TemplateVariables) : obj =
        match node with
        | Text t -> t :> obj
        | Section (name, content) ->
            let contentObjs = content |> List.map (fun n -> nodeToObj n templateVars)
            dict [name, contentObjs :> obj] :> obj
        | List items ->
            (items |> List.map (fun n -> nodeToObj n templateVars)) :> obj
        | Imported (path, format, wrapInCodeBlock) ->
            dict [("path", path :> obj); ("format", format.ToString().ToLower() :> obj); ("wrapInCodeBlock", wrapInCodeBlock :> obj)] :> obj
        | Template text ->
            let rendered = Template.renderInline text templateVars
            dict [("template", text :> obj); ("rendered", rendered :> obj)] :> obj
        | TemplateFile path ->
            let rendered = Template.renderFile path templateVars
            dict [("templateFile", path :> obj); ("rendered", rendered :> obj)] :> obj

    let private writeMd (agent: Agent) (opts: Options) (ctx: WriterContext) : string =
        let sb = StringBuilder()

        // Frontmatter
        if opts.IncludeFrontmatter then
            match opts.OutputFormat with
            | Opencode ->
                if agent.Frontmatter.Count > 0 then
                    sb.AppendLine("---") |> ignore
                    for kv in agent.Frontmatter do
                        let valueStr =
                            match kv.Value with
                            | :? string as s -> s
                            | :? float as f -> f.ToString()
                            | :? bool as b -> b.ToString().ToLower()
                            | :? (obj list) as l ->
                                l |> List.map (fun o ->
                                    match o with
                                    | :? string as s -> s
                                    | _ -> o.ToString()
                                ) |> String.concat "\n  - "
                                |> sprintf "\n  - %s"
                            | :? Map<string, obj> as m ->
                                m |> Map.toSeq |> Seq.map (fun (k,v) -> $"  {k}: {v}") |> String.concat "\n" |> sprintf "\n%s"
                            | _ -> kv.Value.ToString()
                        sb.AppendLine($"{kv.Key}: {valueStr}") |> ignore
                    sb.AppendLine("---") |> ignore
                    sb.AppendLine() |> ignore
            | Copilot ->
                sb.AppendLine("---") |> ignore
                for kv in agent.Frontmatter do
                    let valueStr =
                        match kv.Value with
                        | :? string as s -> s
                        | :? float as f -> f.ToString()
                        | :? bool as b -> b.ToString().ToLower()
                        | :? (obj list) as l ->
                            l |> List.map (fun o ->
                                match o with
                                | :? string as s -> s
                                | _ -> o.ToString()
                            ) |> String.concat "\n  - "
                            |> sprintf "\n  - %s"
                        | :? Map<string, obj> as m ->
                            m |> Map.toSeq |> Seq.map (fun (k,v) -> $"  {k}: {v}") |> String.concat "\n" |> sprintf "\n%s"
                        | _ -> kv.Value.ToString()
                    sb.AppendLine($"{kv.Key}: {valueStr}") |> ignore
                sb.AppendLine("---") |> ignore
                sb.AppendLine() |> ignore

        // Sections
        let rec writeNode (node: Node) (level: int) =
            match node with
            | Text t -> sb.AppendLine(t) |> ignore
            | Section (name, content) ->
                // Add blank line before level-1 headings if not at document start
                if level = 1 then
                    let str = sb.ToString()
                    if str.Length > 0 && not (str.EndsWith("\n\n")) then
                        sb.AppendLine() |> ignore
                let displayName =
                    opts.RenameMap |> Map.tryFind name |> Option.defaultValue name
                    |> (opts.HeadingFormatter |> Option.defaultValue id)
                let heading = String.replicate level "#" + " " + displayName
                sb.AppendLine(heading) |> ignore
                sb.AppendLine() |> ignore
                for c in content do writeNode c (level + 1)
            | List items ->
                for item in items do
                    sb.Append("- ") |> ignore
                    writeNode item level
                    sb.AppendLine() |> ignore
            | Imported (path, format, wrapInCodeBlock) ->
                let shouldWrap = wrapInCodeBlock && not opts.DisableCodeBlockWrapping
                // Add blank line before code blocks if not at section start
                if shouldWrap then
                    let str = sb.ToString()
                    if str.Length > 0 && not (str.EndsWith("\n\n")) then
                        sb.AppendLine() |> ignore
                try
                    let content = System.IO.File.ReadAllText(path)
                    if shouldWrap then
                        let langTag = formatToLanguageTag format
                        sb.AppendLine($"```{langTag}") |> ignore
                        sb.Append(content) |> ignore
                        if not (content.EndsWith("\n")) then
                            sb.AppendLine() |> ignore
                        sb.AppendLine("```") |> ignore
                    else
                        sb.AppendLine(content) |> ignore
                with
                | _ -> sb.AppendLine($"[Error loading {path}]") |> ignore
            | Template text ->
                let rendered = Template.renderInline text opts.TemplateVariables
                sb.AppendLine(rendered) |> ignore
            | TemplateFile path ->
                let rendered = Template.renderFile path opts.TemplateVariables
                sb.AppendLine(rendered) |> ignore

        for section in agent.Sections do
            writeNode section 1

        // Footer
        if opts.GeneratedFooter.IsSome then
            let footer = opts.GeneratedFooter.Value ctx
            sb.AppendLine(footer) |> ignore

        sb.ToString()

    let private writeJson (agent: Agent) (opts: Options) (ctx: WriterContext) : string =
        let data = dict [
            "frontmatter", agent.Frontmatter :> obj
            "sections", (agent.Sections |> List.map (fun n -> nodeToObj n opts.TemplateVariables)) :> obj
        ]
        JsonSerializer.Serialize(data, JsonSerializerOptions(WriteIndented = true))

    let private writeYaml (agent: Agent) (opts: Options) (ctx: WriterContext) : string =
        let serializer = Serializer()
        let data = dict [
            "frontmatter", agent.Frontmatter :> obj
            "sections", (agent.Sections |> List.map (fun n -> nodeToObj n opts.TemplateVariables)) :> obj
        ]
        serializer.Serialize(data)

    let writeAgent (agent: Agent) (configure: Options -> unit) : string =
        let opts = defaultOptions()
        configure opts

        // Validation
        match opts.OutputFormat with
        | Copilot ->
            let name = agent.Frontmatter.TryFind "name" |> Option.map string
            let desc = agent.Frontmatter.TryFind "description" |> Option.map string
            if name.IsNone || desc.IsNone then
                failwith "Copilot format requires 'name' and 'description' in frontmatter"
        | _ -> ()

        if opts.CustomWriter.IsSome then
            opts.CustomWriter.Value agent opts
        else
            let agentName = agent.Frontmatter.TryFind "name" |> Option.map string
            let agentDescription = agent.Frontmatter.TryFind "description" |> Option.map string
            let ctx = {
                Format = opts.OutputFormat
                OutputType = opts.OutputType
                Timestamp = DateTime.Now
                AgentName = agentName
                AgentDescription = agentDescription
            }
            match opts.OutputType with
            | Md -> writeMd agent opts ctx
            | Json -> writeJson agent opts ctx
            | Yaml -> writeYaml agent opts ctx

    let writePrompt (prompt: Prompt) (configure: Options -> unit) : string =
        let opts = defaultOptions()
        configure opts

        // Convert Prompt to Agent-like structure without frontmatter
        let agentLike = { Frontmatter = Map.empty; Sections = prompt.Sections }

        let ctx = {
            Format = opts.OutputFormat
            OutputType = opts.OutputType
            Timestamp = DateTime.Now
            AgentName = None
            AgentDescription = None
        }

        match opts.OutputType with
        | Md -> writeMd agentLike opts ctx
        | Json -> writeJson agentLike opts ctx
        | Yaml -> writeYaml agentLike opts ctx

    // Backward compatibility alias
    let writeMarkdown = writeAgent
