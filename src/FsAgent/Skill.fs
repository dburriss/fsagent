namespace FsAgent.Skills

open FsAgent.AST
open FsAgent.Prompts

type Skill = {
    Frontmatter: Map<string, obj>
    Sections: Node list
}

module Skill =
    let empty : Skill = { Frontmatter = Map.empty; Sections = [] }

[<AutoOpen>]
module SkillBuilder =
    type SkillBuilder() =
        member _.Yield _ = Skill.empty

        member _.Zero() = Skill.empty

        member _.Run(s) = s

        [<CustomOperation("name")>]
        member _.Name(s: Skill, value: string) =
            { s with Frontmatter = s.Frontmatter |> Map.add "name" (value :> obj) }

        [<CustomOperation("description")>]
        member _.Description(s: Skill, value: string) =
            { s with Frontmatter = s.Frontmatter |> Map.add "description" (value :> obj) }

        [<CustomOperation("license")>]
        member _.License(s: Skill, value: string) =
            { s with Frontmatter = s.Frontmatter |> Map.add "license" (value :> obj) }

        [<CustomOperation("compatibility")>]
        member _.Compatibility(s: Skill, value: string) =
            { s with Frontmatter = s.Frontmatter |> Map.add "compatibility" (value :> obj) }

        [<CustomOperation("metadata")>]
        member _.Metadata(s: Skill, value: Map<string, obj>) =
            { s with Frontmatter = s.Frontmatter |> Map.add "metadata" (value :> obj) }

        [<CustomOperation("section")>]
        member _.Section(s: Skill, name: string, content: string) =
            { s with Sections = s.Sections @ [Section(name, [Text content])] }

        [<CustomOperation("prompt")>]
        member _.Prompt(s: Skill, prompt: Prompt) =
            { s with Sections = s.Sections @ prompt.Sections }

        [<CustomOperation("import")>]
        member _.Import(s: Skill, path: string) =
            { s with Sections = s.Sections @ [AST.importRef path] }

        [<CustomOperation("importRaw")>]
        member _.ImportRaw(s: Skill, path: string) =
            { s with Sections = s.Sections @ [AST.importRawRef path] }

        [<CustomOperation("template")>]
        member _.Template(s: Skill, text: string) =
            { s with Sections = s.Sections @ [Template text] }

        [<CustomOperation("templateFile")>]
        member _.TemplateFile(s: Skill, path: string) =
            { s with Sections = s.Sections @ [TemplateFile path] }

    let skill = SkillBuilder()
