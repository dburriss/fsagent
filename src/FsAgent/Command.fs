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

        [<CustomOperation("prompt")>]
        member _.Prompt(cmd: SlashCommand, prompt: Prompt) =
            { cmd with Sections = cmd.Sections @ prompt.Sections }

    let command = CommandBuilder()
