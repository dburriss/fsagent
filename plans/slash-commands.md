# Plan: Add Slash Command Support

Status: Draft

## Overview

Add `SlashCommand` as a first-class type alongside `Agent` and `Prompt`. It follows the same 4-layer stratified design: DSL → AST (reuses `Node`) → Writer → output. A slash command is conceptually a named prompt with a `description` frontmatter key; its name is used only for output file naming. All current harnesses (Opencode, Copilot, ClaudeCode) use the same Markdown format.

No changes to the existing `Agent`, `Prompt`, or `AST` layers. No breaking changes.

## Key Design Decisions

- **`SlashCommand` is a dedicated type** — not a wrapper around `Agent`. It holds `Name: string`, `Description: string`, and `Sections: Node list`.
- **Name is file-naming metadata only** — not included in frontmatter (matches existing `.opencode/command/*.md` pattern).
- **`description` in frontmatter** — a frontmatter key; matches observed harness format.
- **Sections reuse `Node`** — no new AST nodes. All existing DSL operations (`template`, `import`, `section`, etc.) apply.
- **Writer produces Markdown** by default; `OutputType` option (Md/Json/Yaml) follows the same pattern as `renderAgent` for completeness.
- **Harness differences**: None currently — all three emit the same format. The writer accepts `AgentHarness` in options for future-proofing; no conditional output yet.
- **Template rendering** at write time; uses `Template.renderWithHarness` just like `renderMd`.

## DSL Example (Target API)

```fsharp
open FsAgent.Commands
open FsAgent.Writers

let myCmd =
    command {
        name "my-command"          // used for file naming only
        description "Does a thing" // → frontmatter
        instructions "Step 1: ..."
        template "Use {{{tool Bash}}} to run commands."
        import "shared/context.md"
    }

// Render to string
let output = AgentWriter.renderCommand myCmd (fun opts ->
    opts.OutputFormat <- Opencode)
// → ---\ndescription: Does a thing\n---\n\n# instructions\n...
```

## Type Definition

**New file: `src/FsAgent/Command.fs`** — namespace `FsAgent.Commands`

```fsharp
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
        member _.Name(cmd, value: string) = { cmd with Name = value }

        [<CustomOperation("description")>]
        member _.Description(cmd, value: string) = { cmd with Description = value }

        [<CustomOperation("role")>]
        member _.Role(cmd, text: string) =
            { cmd with Sections = cmd.Sections @ [Prompt.role text] }

        [<CustomOperation("objective")>]
        member _.Objective(cmd, text: string) =
            { cmd with Sections = cmd.Sections @ [Prompt.objective text] }

        [<CustomOperation("instructions")>]
        member _.Instructions(cmd, text: string) =
            { cmd with Sections = cmd.Sections @ [Prompt.instructions text] }

        [<CustomOperation("context")>]
        member _.Context(cmd, text: string) =
            { cmd with Sections = cmd.Sections @ [Prompt.context text] }

        [<CustomOperation("output")>]
        member _.Output(cmd, text: string) =
            { cmd with Sections = cmd.Sections @ [Prompt.output text] }

        [<CustomOperation("section")>]
        member _.Section(cmd, name: string, content: string) =
            { cmd with Sections = cmd.Sections @ [Section(name, [Text content])] }

        [<CustomOperation("import")>]
        member _.Import(cmd, path: string) =
            { cmd with Sections = cmd.Sections @ [AST.importRef path] }

        [<CustomOperation("importRaw")>]
        member _.ImportRaw(cmd, path: string) =
            { cmd with Sections = cmd.Sections @ [AST.importRawRef path] }

        [<CustomOperation("template")>]
        member _.Template(cmd, text: string) =
            { cmd with Sections = cmd.Sections @ [Template text] }

        [<CustomOperation("templateFile")>]
        member _.TemplateFile(cmd, path: string) =
            { cmd with Sections = cmd.Sections @ [TemplateFile path] }

        [<CustomOperation("examples")>]
        member _.Examples(cmd, examples: Node list) =
            { cmd with Sections = cmd.Sections @ [Prompt.examples examples] }

        [<CustomOperation("prompt")>]
        member _.Prompt(cmd, p: Prompt) =
            { cmd with Sections = cmd.Sections @ p.Sections }

    let command = CommandBuilder()
```

## Writer Extension

Add `renderCommand` to `AgentWriter` module in **`Writers.fs`**:

```fsharp
let renderCommand (cmd: SlashCommand) (configure: Options -> unit) : string =
    let opts = defaultOptions()
    configure opts
    // Build a synthetic Agent with only `description` in frontmatter
    let agentLike = {
        Frontmatter = Map.ofList ["description", AST.fmStr cmd.Description]
        Sections = cmd.Sections
    }
    let ctx = {
        Format = opts.OutputFormat
        OutputType = opts.OutputType
        Timestamp = DateTime.Now
        AgentName = Some cmd.Name
        AgentDescription = Some cmd.Description
    }
    match opts.OutputType with
    | Md -> renderMd agentLike opts ctx
    | Json -> renderJson agentLike opts ctx
    | Yaml -> renderYaml agentLike opts ctx
```

No new private rendering logic needed — `renderMd` already handles sections, templates, imports, and `description`-only frontmatter correctly.

## Library.fs Backward Compatibility Layer

Add re-exports in `Library.fs`:

```fsharp
type SlashCommand = Commands.SlashCommand

module Commands =
    let command = Commands.command
```

## Compilation Order

Update `FsAgent.fsproj` — insert `Command.fs` after `Agent.fs`:

```xml
<Compile Include="Tools.fs" />
<Compile Include="AST.fs" />
<Compile Include="Prompt.fs" />
<Compile Include="Agent.fs" />
<Compile Include="Command.fs" />   <!-- NEW -->
<Compile Include="Writers.fs" />
<Compile Include="Library.fs" />
```

## Tests

**New file: `tests/FsAgent.Tests/CommandTests.fs`** (category A)

| Test | Description |
|------|-------------|
| `command with description renders frontmatter` | `description` appears in YAML front block |
| `command name is not in frontmatter` | `name` absent from output |
| `command sections render correctly` | `role`, `instructions`, `section` appear as headings |
| `command template renders with harness` | `{{{tool Bash}}}` resolves per harness |
| `command import embeds file` | `import` node works as in agent |
| `command renders for Copilot` | Same output format as Opencode (no diff) |
| `command renders for ClaudeCode` | Same output format as Opencode (no diff) |
| `empty command renders only description frontmatter` | Minimal valid output |

## Files to Create / Modify

| File | Action | Notes |
|------|--------|-------|
| `src/FsAgent/Command.fs` | Create | `SlashCommand` type + `CommandBuilder` CE |
| `src/FsAgent/FsAgent.fsproj` | Modify | Add `Command.fs` compile entry |
| `src/FsAgent/Writers.fs` | Modify | Add `renderCommand` to `AgentWriter` |
| `src/FsAgent/Library.fs` | Modify | Add `SlashCommand` alias + `Commands` re-export |
| `tests/FsAgent.Tests/CommandTests.fs` | Create | Acceptance tests |
| `tests/FsAgent.Tests/FsAgent.Tests.fsproj` | Modify | Add `CommandTests.fs` compile entry |
| `CHANGELOG.md` | Modify | Entry under `[Unreleased]` |

## Open Questions

1. ~~Should `command { ... }` also support `examples` / `prompt promptRef` operations (composing a `Prompt` into a command), for symmetry with `agent`?~~ **Yes — add `prompt` and `examples` operations.**
2. ~~Should `writeCommand` validate that `Name` is non-empty (and optionally that it is valid kebab-case)?~~ **Deferred — validation will be added to all builders in a future change.**
