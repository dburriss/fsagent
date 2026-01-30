namespace FsAgent

open System
open System.Collections.Generic
open System.Text
open System.Text.Json
open YamlDotNet.Serialization

type DataFormat =
    | Yaml
    | Json
    | Toon
    | Unknown

type Node =
    | Text of string
    | Section of name: string * content: Node list
    | List of Node list
    | Imported of sourcePath: string * format: DataFormat

type Agent = {
    Frontmatter: Map<string, obj>
    Sections: Node list
}

module AST =
    let role (text: string) : Node =
        Section("role", [Text text])

    let objective (text: string) : Node =
        Section("objective", [Text text])

    let instructions (text: string) : Node =
        Section("instructions", [Text text])

    let context (text: string) : Node =
        Section("context", [Text text])

    let output (text: string) : Node =
        Section("output", [Text text])

    let example (title: string) (content: string) : Node =
        Section("example", [Text title; Text content])

    let examples (examples: Node list) : Node =
        Section("examples", [List examples])

    let fmStr (value: string) : obj = value :> obj

    let fmNum (value: float) : obj = value :> obj

    let fmBool (value: bool) : obj = value :> obj

    let fmList (value: obj list) : obj = value :> obj

    let fmMap (value: Map<string, obj>) : obj = value :> obj

    let inferFormat (path: string) : DataFormat =
        match System.IO.Path.GetExtension(path).ToLower() with
        | ".yml" | ".yaml" -> Yaml
        | ".json" -> Json
        | ".toon" -> Toon
        | _ -> Unknown

    let importRef (path: string) : Node =
        Imported(path, inferFormat path)

module DSL =

    type MetaBuilder() =
        member _.Yield _ = Map.empty<string, obj>

        member _.Run(map) = map

        [<CustomOperation("kv")>]
        member _.Kv(map, key: string, value: string) =
            map |> Map.add key (AST.fmStr value)

        [<CustomOperation("kvList")>]
        member _.KvList(map, key: string, value: string list) =
            map |> Map.add key (AST.fmList (value |> List.map box))

        [<CustomOperation("kvObj")>]
        member _.KvObj(map, key: string, value: Map<string, obj>) =
            map |> Map.add key (AST.fmMap value)

        [<CustomOperation("kvListObj")>]
        member _.KvListObj(map, key: string, value: obj list) =
            map |> Map.add key (AST.fmList value)

    let meta = MetaBuilder()

    type AgentBuilder() =
        member _.Yield _ = { Frontmatter = Map.empty; Sections = [] }

        member _.Run(agent) = agent

        [<CustomOperation("meta")>]
        member _.Meta(agent, frontmatter: Map<string, obj>) =
            { agent with Frontmatter = frontmatter }

        [<CustomOperation("role")>]
        member _.Role(agent, text: string) =
            { agent with Sections = agent.Sections @ [AST.role text] }

        [<CustomOperation("objective")>]
        member _.Objective(agent, text: string) =
            { agent with Sections = agent.Sections @ [AST.objective text] }

        [<CustomOperation("instructions")>]
        member _.Instructions(agent, text: string) =
            { agent with Sections = agent.Sections @ [AST.instructions text] }

        [<CustomOperation("context")>]
        member _.Context(agent, text: string) =
            { agent with Sections = agent.Sections @ [AST.context text] }

        [<CustomOperation("output")>]
        member _.Output(agent, text: string) =
            { agent with Sections = agent.Sections @ [AST.output text] }

        [<CustomOperation("examples")>]
        member _.Examples(agent, examples: Node list) =
            { agent with Sections = agent.Sections @ [AST.examples examples] }

        [<CustomOperation("section")>]
        member _.Section(agent, name: string, content: string) =
            { agent with Sections = agent.Sections @ [Section(name, [Text content])] }

        [<CustomOperation("importRaw")>]
        member _.ImportRaw(agent, path: string) =
            { agent with Sections = agent.Sections @ [AST.importRef path] }

        [<CustomOperation("import")>]
        member _.Import(agent, path: string) =
            { agent with Sections = agent.Sections @ [AST.importRef path] }

    let agent = AgentBuilder()

module MarkdownWriter =

    type AgentFormat =
        | Opencode
        | Copilot

    type OutputType =
        | Md
        | Json
        | Yaml

    type ImportInclusion =
        | Exclude
        | IncludeRaw
        | IncludeCodeBlock

    type WriterContext = {
        Format: AgentFormat
        OutputType: OutputType
        Timestamp: System.DateTime
        AgentName: string option
        AgentDescription: string option
    }

    type Options = {
        mutable OutputFormat: AgentFormat
        mutable OutputType: OutputType
        mutable ImportInclusion: ImportInclusion
        mutable RenameMap: Map<string, string>
        mutable HeadingFormatter: (string -> string) option
        mutable GeneratedFooter: (WriterContext -> string) option
        mutable IncludeFrontmatter: bool
        mutable CustomWriter: (Agent -> Options -> string) option
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
        ImportInclusion = Exclude
        RenameMap = Map.empty
        HeadingFormatter = None
        GeneratedFooter = None
        IncludeFrontmatter = true
        CustomWriter = None
    }

    let rec nodeToObj (node: Node) : obj =
        match node with
        | Text t -> t :> obj
        | Section (name, content) ->
            let contentObjs = content |> List.map nodeToObj
            dict [name, contentObjs :> obj] :> obj
        | List items ->
            (items |> List.map nodeToObj) :> obj
        | Imported (path, format) ->
            dict ["path", path; "format", format.ToString().ToLower()] :> obj

    let private writeMd (agent: Agent) (opts: Options) (ctx: WriterContext) : string =
        let sb = System.Text.StringBuilder()

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
            | Imported (path, format) ->
                match opts.ImportInclusion with
                | Exclude -> ()
                | IncludeRaw ->
                    try
                        let content = System.IO.File.ReadAllText(path)
                        sb.AppendLine(content) |> ignore
                    with
                    | _ -> sb.AppendLine($"[Error loading {path}]") |> ignore
                | IncludeCodeBlock ->
                    try
                        let content = System.IO.File.ReadAllText(path)
                        let langTag = formatToLanguageTag format
                        sb.AppendLine($"```{langTag}") |> ignore
                        sb.Append(content) |> ignore
                        if not (content.EndsWith("\n")) then
                            sb.AppendLine() |> ignore
                        sb.AppendLine("```") |> ignore
                    with
                    | _ -> sb.AppendLine($"[Error loading {path}]") |> ignore

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
            "sections", (agent.Sections |> List.map nodeToObj) :> obj
        ]
        System.Text.Json.JsonSerializer.Serialize(data, System.Text.Json.JsonSerializerOptions(WriteIndented = true))

    let private writeYaml (agent: Agent) (opts: Options) (ctx: WriterContext) : string =
        let serializer = YamlDotNet.Serialization.Serializer()
        let data = dict [
            "frontmatter", agent.Frontmatter :> obj
            "sections", (agent.Sections |> List.map nodeToObj) :> obj
        ]
        serializer.Serialize(data)

    let writeMarkdown (agent: Agent) (configure: Options -> unit) : string =
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
                Timestamp = System.DateTime.Now
                AgentName = agentName
                AgentDescription = agentDescription
            }
            match opts.OutputType with
            | Md -> writeMd agent opts ctx
            | Json -> writeJson agent opts ctx
            | Yaml -> writeYaml agent opts ctx

