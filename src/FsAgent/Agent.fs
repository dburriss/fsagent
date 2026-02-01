namespace FsAgent.Agents

open FsAgent.AST
open FsAgent.Prompts

type Agent = {
    Frontmatter: Map<string, obj>
    Sections: Node list
}

module Agent =
    let empty : Agent = { Frontmatter = Map.empty; Sections = [] }

[<AutoOpen>]
module AgentBuilder =
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
        member _.Yield _ = Agent.empty

        member _.Zero() = Agent.empty

        member _.Run(agent) = agent

        [<CustomOperation("meta")>]
        member _.Meta(agent, frontmatter: Map<string, obj>) =
            { agent with Frontmatter = frontmatter }

        [<CustomOperation("name")>]
        member _.Name(agent, value: string) =
            { agent with Frontmatter = agent.Frontmatter |> Map.add "name" (AST.fmStr value) }

        [<CustomOperation("description")>]
        member _.Description(agent, value: string) =
            { agent with Frontmatter = agent.Frontmatter |> Map.add "description" (AST.fmStr value) }

        [<CustomOperation("author")>]
        member _.Author(agent, value: string) =
            { agent with Frontmatter = agent.Frontmatter |> Map.add "author" (AST.fmStr value) }

        [<CustomOperation("version")>]
        member _.Version(agent, value: string) =
            { agent with Frontmatter = agent.Frontmatter |> Map.add "version" (AST.fmStr value) }

        [<CustomOperation("license")>]
        member _.License(agent, value: string) =
            { agent with Frontmatter = agent.Frontmatter |> Map.add "license" (AST.fmStr value) }

        [<CustomOperation("model")>]
        member _.Model(agent, value: string) =
            { agent with Frontmatter = agent.Frontmatter |> Map.add "model" (AST.fmStr value) }

        [<CustomOperation("temperature")>]
        member _.Temperature(agent, value: float) =
            { agent with Frontmatter = agent.Frontmatter |> Map.add "temperature" (AST.fmNum value) }

        [<CustomOperation("maxTokens")>]
        member _.MaxTokens(agent, value: float) =
            { agent with Frontmatter = agent.Frontmatter |> Map.add "maxTokens" (AST.fmNum value) }

        [<CustomOperation("tools")>]
        member _.Tools(agent, value: obj list) =
            { agent with Frontmatter = agent.Frontmatter |> Map.add "tools" (AST.fmList value) }

        [<CustomOperation("toolMap")>]
        member _.ToolMap(agent, value: (string * bool) list) =
            let toolMap = value |> List.map (fun (k, v) -> (k, v :> obj)) |> Map.ofList
            { agent with Frontmatter = agent.Frontmatter |> Map.add "tools" (AST.fmMap toolMap) }

        [<CustomOperation("disallowedTools")>]
        member _.DisallowedTools(agent, value: string list) =
            // Get existing tools configuration
            let existingTools = agent.Frontmatter |> Map.tryFind "tools"

            let toolMap =
                match existingTools with
                // If existing tools is a list, convert to map with all true, then add disallowed
                | Some (:? (obj list) as toolList) ->
                    let allowedMap =
                        toolList
                        |> List.map (fun t ->
                            let name = match t with :? string as s -> s | _ -> t.ToString()
                            (name, true :> obj))
                        |> Map.ofList
                    let disallowedMap =
                        value
                        |> List.map (fun name -> (name, false :> obj))
                        |> Map.ofList
                    Map.fold (fun acc k v -> Map.add k v acc) allowedMap disallowedMap

                // If existing tools is a map, add disallowed tools to it
                | Some (:? Map<string, obj> as existingMap) ->
                    let disallowedMap =
                        value
                        |> List.map (fun name -> (name, false :> obj))
                        |> Map.ofList
                    Map.fold (fun acc k v -> Map.add k v acc) existingMap disallowedMap

                // If no existing tools, create map with only disallowed
                | _ ->
                    value
                    |> List.map (fun name -> (name, false :> obj))
                    |> Map.ofList

            { agent with Frontmatter = agent.Frontmatter |> Map.add "tools" (AST.fmMap toolMap) }

        [<CustomOperation("prompt")>]
        member _.Prompt(agent, prompt: Prompt) =
            { agent with Sections = agent.Sections @ prompt.Sections }

        [<CustomOperation("section")>]
        member _.Section(agent, name: string, content: string) =
            { agent with Sections = agent.Sections @ [Section(name, [Text content])] }

        [<CustomOperation("import")>]
        member _.Import(agent, path: string) =
            { agent with Sections = agent.Sections @ [AST.importRef path] }

        [<CustomOperation("importRaw")>]
        member _.ImportRaw(agent, path: string) =
            { agent with Sections = agent.Sections @ [AST.importRawRef path] }

    let agent = AgentBuilder()
