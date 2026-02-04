namespace FsAgent.Writers

open System
open System.Text
open System.Text.Json
open YamlDotNet.Serialization
open FsAgent.AST
open FsAgent.Tools
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

    type AgentHarness =
        | Opencode
        | Copilot
        | ClaudeCode

    type OutputType =
        | Md
        | Json
        | Yaml

    type WriterContext = {
        Format: AgentHarness
        OutputType: OutputType
        Timestamp: DateTime
        AgentName: string option
        AgentDescription: string option
    }

    type Options = {
        mutable OutputFormat: AgentHarness
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
        | Node.List items ->
            (items |> List.map (fun n -> nodeToObj n templateVars)) :> obj
        | Imported (path, format, wrapInCodeBlock) ->
            dict [("path", path :> obj); ("format", format.ToString().ToLower() :> obj); ("wrapInCodeBlock", wrapInCodeBlock :> obj)] :> obj
        | Template text ->
            let rendered = Template.renderInline text templateVars
            dict [("template", text :> obj); ("rendered", rendered :> obj)] :> obj
        | TemplateFile path ->
            let rendered = Template.renderFile path templateVars
            dict [("templateFile", path :> obj); ("rendered", rendered :> obj)] :> obj

    let private toolToString (harness: AgentHarness) (tool: Tool) : string list =
        match harness, tool with
        // Opencode tools (lowercase)
        | Opencode, Tool.Write -> ["write"]
        | Opencode, Tool.Edit -> ["edit"]
        | Opencode, Tool.Bash -> ["bash"]
        | Opencode, Tool.Shell -> ["bash"]
        | Opencode, Tool.Read -> ["read"]
        | Opencode, Tool.Glob -> ["grep"]
        | Opencode, Tool.List -> ["list"]
        | Opencode, Tool.LSP -> ["lsp"]
        | Opencode, Tool.Skill -> ["skill"]
        | Opencode, Tool.TodoWrite -> ["todowrite"]
        | Opencode, Tool.TodoRead -> ["todoread"]
        | Opencode, Tool.WebFetch -> ["webfetch"]
        | Opencode, Tool.WebSearch -> []  // Not supported
        | Opencode, Tool.Question -> ["question"]
        | Opencode, Tool.Todo -> ["todo"]

        // Copilot tools
        | Copilot, Tool.Write -> ["write"]
        | Copilot, Tool.Edit -> ["edit"]
        | Copilot, Tool.Bash -> ["bash"]
        | Copilot, Tool.Shell -> ["execute"]
        | Copilot, Tool.Read -> ["read"]
        | Copilot, Tool.Glob -> ["search"]
        | Copilot, Tool.List -> ["search"]
        | Copilot, Tool.LSP -> []  // Not supported
        | Copilot, Tool.Skill -> ["skill"]
        | Copilot, Tool.TodoWrite -> ["todo"]
        | Copilot, Tool.TodoRead -> ["todo"]
        | Copilot, Tool.WebFetch -> ["web"]
        | Copilot, Tool.WebSearch -> ["web"]
        | Copilot, Tool.Question -> []  // Not supported
        | Copilot, Tool.Todo -> ["todo"]

        // ClaudeCode tools (capitalized)
        | ClaudeCode, Tool.Write -> ["Write"]
        | ClaudeCode, Tool.Edit -> ["Edit"]
        | ClaudeCode, Tool.Bash -> ["Bash"]
        | ClaudeCode, Tool.Shell -> ["Bash"]
        | ClaudeCode, Tool.Read -> ["Read"]
        | ClaudeCode, Tool.Glob -> ["Glob"]
        | ClaudeCode, Tool.List -> ["Glob"]
        | ClaudeCode, Tool.LSP -> ["LSP"]
        | ClaudeCode, Tool.Skill -> ["skill"]
        | ClaudeCode, Tool.TodoWrite -> ["TaskCreate"; "TaskUpdate"]
        | ClaudeCode, Tool.TodoRead -> ["TaskList"; "TaskGet"; "TaskUpdate"]
        | ClaudeCode, Tool.WebFetch -> ["WebFetch"]
        | ClaudeCode, Tool.WebSearch -> ["WebSearch"]
        | ClaudeCode, Tool.Question -> ["AskUserQuestion"]
        | ClaudeCode, Tool.Todo -> ["Todo"]

        // Custom tools pass through unchanged for all harnesses
        | _, Tool.Custom s -> [s]

    let private formatToolsFrontmatter (frontmatter: Map<string, obj>) (harness: AgentHarness) : string =
        // Extract Tool lists from frontmatter
        let enabledTools =
            match frontmatter |> Map.tryFind "tools" with
            | Some value ->
                match value with
                | :? (Tool list) as tools -> tools
                | :? (obj list) as lst ->
                    lst |> List.choose (function :? Tool as t -> Some t | _ -> None)
                | _ -> []
            | None -> []

        let disabledTools =
            match frontmatter |> Map.tryFind "disallowedTools" with
            | Some value ->
                match value with
                | :? (Tool list) as tools -> tools
                | :? (obj list) as lst ->
                    lst |> List.choose (function :? Tool as t -> Some t | _ -> None)
                | _ -> []
            | None -> []

        // Convert to string maps using harness-specific names
        let enabledMap =
            enabledTools
            |> List.collect (toolToString harness)
            |> List.distinct
            |> List.map (fun t -> (t, true :> obj))
            |> Map.ofList

        let disabledMap =
            disabledTools
            |> List.collect (toolToString harness)
            |> List.distinct
            |> List.map (fun t -> (t, false :> obj))
            |> Map.ofList

        // Merge: disabled tools override enabled ones
        let toolMap =
            Map.fold (fun acc k v -> Map.add k v acc) enabledMap disabledMap

        // Output format depends on harness
        match harness with
        | Opencode ->
            // Struct format: include all tools with true/false values
            toolMap
            |> Map.toSeq
            |> Seq.sortBy fst  // Sort alphabetically for deterministic output
            |> Seq.map (fun (k, v) ->
                let valueStr =
                    match v with
                    | :? bool as b -> b.ToString().ToLower()
                    | _ -> "true"
                sprintf "  %s: %s" k valueStr)
            |> String.concat "\n"
            |> sprintf "\n%s"

        | Copilot | ClaudeCode ->
            // List format: only include enabled tools
            let enabledTools =
                toolMap
                |> Map.filter (fun _ v ->
                    match v with
                    | :? bool as b -> b
                    | _ -> true)
                |> Map.keys
                |> Seq.map string
                |> Seq.toList

            if enabledTools.IsEmpty then
                ""  // No tools to output
            else
                enabledTools
                |> String.concat "\n  - "
                |> sprintf "\n  - %s"

    let private writeMd (agent: Agent) (opts: Options) (ctx: WriterContext) : string =
        let sb = StringBuilder()

        // Frontmatter
        if opts.IncludeFrontmatter then
            match opts.OutputFormat with
            | Opencode ->
                if agent.Frontmatter.Count > 0 then
                    sb.AppendLine("---") |> ignore

                    // Handle tools section first (if tools or disallowedTools exist)
                    let hasTools = agent.Frontmatter.ContainsKey("tools") || agent.Frontmatter.ContainsKey("disallowedTools")
                    if hasTools then
                        let toolsStr = formatToolsFrontmatter agent.Frontmatter Opencode
                        sb.AppendLine($"tools: {toolsStr}") |> ignore

                    // Handle other frontmatter keys
                    for kv in agent.Frontmatter do
                        if kv.Key <> "tools" && kv.Key <> "disallowedTools" then
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

                // Handle tools section first (if tools or disallowedTools exist)
                let hasTools = agent.Frontmatter.ContainsKey("tools") || agent.Frontmatter.ContainsKey("disallowedTools")
                if hasTools then
                    let toolsStr = formatToolsFrontmatter agent.Frontmatter Copilot
                    if toolsStr <> "" then
                        sb.AppendLine($"tools: {toolsStr}") |> ignore

                // Handle other frontmatter keys
                for kv in agent.Frontmatter do
                    if kv.Key <> "tools" && kv.Key <> "disallowedTools" then
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
            | ClaudeCode ->
                if agent.Frontmatter.Count > 0 then
                    sb.AppendLine("---") |> ignore

                    // Handle tools section first (if tools or disallowedTools exist)
                    let hasTools = agent.Frontmatter.ContainsKey("tools") || agent.Frontmatter.ContainsKey("disallowedTools")
                    if hasTools then
                        let toolsStr = formatToolsFrontmatter agent.Frontmatter ClaudeCode
                        if toolsStr <> "" then
                            sb.AppendLine($"tools: {toolsStr}") |> ignore

                    // Handle other frontmatter keys
                    for kv in agent.Frontmatter do
                        if kv.Key <> "tools" && kv.Key <> "disallowedTools" then
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
            | Node.List items ->
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
