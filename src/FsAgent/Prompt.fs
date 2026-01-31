namespace FsAgent.Prompts

open FsAgent.AST

type Prompt = {
    Frontmatter: Map<string, obj>
    Sections: Node list
}

module Prompt =
    let empty : Prompt = { Frontmatter = Map.empty; Sections = [] }

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
        Section("examples", examples)

[<AutoOpen>]
module PromptBuilder =
    type PromptBuilder() =
        member _.Yield _ = Prompt.empty

        member _.Zero() = Prompt.empty

        member _.Run(prompt) = prompt

        [<CustomOperation("meta")>]
        member _.Meta(prompt, frontmatter: Map<string, obj>) =
            { prompt with Frontmatter = frontmatter }

        [<CustomOperation("name")>]
        member _.Name(prompt, value: string) =
            { prompt with Frontmatter = prompt.Frontmatter |> Map.add "name" (AST.fmStr value) }

        [<CustomOperation("description")>]
        member _.Description(prompt, value: string) =
            { prompt with Frontmatter = prompt.Frontmatter |> Map.add "description" (AST.fmStr value) }

        [<CustomOperation("author")>]
        member _.Author(prompt, value: string) =
            { prompt with Frontmatter = prompt.Frontmatter |> Map.add "author" (AST.fmStr value) }

        [<CustomOperation("version")>]
        member _.Version(prompt, value: string) =
            { prompt with Frontmatter = prompt.Frontmatter |> Map.add "version" (AST.fmStr value) }

        [<CustomOperation("license")>]
        member _.License(prompt, value: string) =
            { prompt with Frontmatter = prompt.Frontmatter |> Map.add "license" (AST.fmStr value) }

        [<CustomOperation("role")>]
        member _.Role(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Prompt.role text] }

        [<CustomOperation("objective")>]
        member _.Objective(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Prompt.objective text] }

        [<CustomOperation("instructions")>]
        member _.Instructions(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Prompt.instructions text] }

        [<CustomOperation("context")>]
        member _.Context(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Prompt.context text] }

        [<CustomOperation("output")>]
        member _.Output(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Prompt.output text] }

        [<CustomOperation("examples")>]
        member _.Examples(prompt, examples: Node list) =
            { prompt with Sections = prompt.Sections @ [Prompt.examples examples] }

        [<CustomOperation("section")>]
        member _.Section(prompt, name: string, content: string) =
            { prompt with Sections = prompt.Sections @ [Section(name, [Text content])] }

        [<CustomOperation("import")>]
        member _.Import(prompt, path: string) =
            { prompt with Sections = prompt.Sections @ [AST.importRef path] }

        [<CustomOperation("importRaw")>]
        member _.ImportRaw(prompt, path: string) =
            { prompt with Sections = prompt.Sections @ [AST.importRawRef path] }

        [<CustomOperation("template")>]
        member _.Template(prompt, text: string) =
            { prompt with Sections = prompt.Sections @ [Template text] }

        [<CustomOperation("templateFile")>]
        member _.TemplateFile(prompt, path: string) =
            { prompt with Sections = prompt.Sections @ [TemplateFile path] }

    let prompt = PromptBuilder()
