namespace FsAgent.Commands

open FsAgent.AST
open FsAgent.Prompts

type SlashCommand = {
    Name: string
    Description: string
    Sections: Node list
}

module SlashCommand =
    let empty : SlashCommand = { Name = ""; Description = ""; Sections = [] }

[<AutoOpen>]
module CommandBuilder =
    type CommandBuilder() =
        member _.Yield _ = SlashCommand.empty

        member _.Zero() = SlashCommand.empty

        member _.Run(cmd) = cmd

        [<CustomOperation("name")>]
        member _.Name(cmd: SlashCommand, value: string) =
            { cmd with Name = value }

        [<CustomOperation("description")>]
        member _.Description(cmd: SlashCommand, value: string) =
            { cmd with Description = value }

        [<CustomOperation("role")>]
        member _.Role(cmd: SlashCommand, text: string) =
            { cmd with Sections = cmd.Sections @ [Prompt.role text] }

        [<CustomOperation("objective")>]
        member _.Objective(cmd: SlashCommand, text: string) =
            { cmd with Sections = cmd.Sections @ [Prompt.objective text] }

        [<CustomOperation("instructions")>]
        member _.Instructions(cmd: SlashCommand, text: string) =
            { cmd with Sections = cmd.Sections @ [Prompt.instructions text] }

        [<CustomOperation("context")>]
        member _.Context(cmd: SlashCommand, text: string) =
            { cmd with Sections = cmd.Sections @ [Prompt.context text] }

        [<CustomOperation("output")>]
        member _.Output(cmd: SlashCommand, text: string) =
            { cmd with Sections = cmd.Sections @ [Prompt.output text] }

        [<CustomOperation("section")>]
        member _.Section(cmd: SlashCommand, name: string, content: string) =
            { cmd with Sections = cmd.Sections @ [Section(name, [Text content])] }

        [<CustomOperation("import")>]
        member _.Import(cmd: SlashCommand, path: string) =
            { cmd with Sections = cmd.Sections @ [AST.importRef path] }

        [<CustomOperation("importRaw")>]
        member _.ImportRaw(cmd: SlashCommand, path: string) =
            { cmd with Sections = cmd.Sections @ [AST.importRawRef path] }

        [<CustomOperation("template")>]
        member _.Template(cmd: SlashCommand, text: string) =
            { cmd with Sections = cmd.Sections @ [Template text] }

        [<CustomOperation("templateFile")>]
        member _.TemplateFile(cmd: SlashCommand, path: string) =
            { cmd with Sections = cmd.Sections @ [TemplateFile path] }

        [<CustomOperation("examples")>]
        member _.Examples(cmd: SlashCommand, examples: Node list) =
            { cmd with Sections = cmd.Sections @ [Prompt.examples examples] }

        [<CustomOperation("prompt")>]
        member _.Prompt(cmd: SlashCommand, prompt: Prompt) =
            { cmd with Sections = cmd.Sections @ prompt.Sections }

    let command = CommandBuilder()
