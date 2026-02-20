# Plan: Skill DSL Support

Status: Draft

## Overview

Add a `skill { ... }` computation expression and `AgentWriter.renderSkill` so FsAgent can programmatically generate `SKILL.md` files compatible with both GitHub Copilot (`.github/skills/`) and OpenCode (`.opencode/skills/`) skill formats.

Follows the same 4-layer stratified design as `SlashCommand`: DSL → `Skill` type → `renderSkill` writer → Markdown output. Output is **Markdown only** — the SKILL.md format has no JSON/YAML variant in either harness. `OutputType` from `Options` is ignored for skill rendering.

No changes to `Agent`, `Prompt`, `Command`, or `Node`. No breaking changes.

## Key Design Decisions

- **`Skill` is a dedicated type** — `Name`, `Description`, `License`, `Compatibility`, `Metadata`, `Sections`.
- **`name` is included in frontmatter** — unlike `SlashCommand` where name is file-naming metadata only; both OpenCode and GitHub Copilot specs require `name` in SKILL.md frontmatter.
- **`metadata` is `Map<string, string>`** — matching the OpenCode spec's string-to-string map constraint.
- **Optional fields omitted when absent** — `license`, `compatibility`, `metadata` are not emitted if unset/empty.
- **Markdown only** — `OutputType` is ignored; SKILL.md has no JSON/YAML representation.
- **Template resolution uses `opts.OutputFormat`** — defaults to `Opencode` via `defaultOptions()`, same pattern as all other writers. Caller can override via `configure` lambda for harness-aware `{{{tool X}}}` resolution.
- **Name validation deferred** — consistent with `SlashCommand`. No kebab-case enforcement at build time.
- **Sections reuse `Node`** — all existing DSL operations (`role`, `instructions`, `section`, `template`, `import`, etc.) apply unchanged.

## DSL Example (Target API)

```fsharp
open FsAgent.Skills
open FsAgent.Writers

let mySkill =
    skill {
        name "git-release"
        description "Create consistent releases and changelogs. Use when preparing a tagged release."
        license "MIT"
        compatibility "opencode"
        meta "author" "me"
        meta "workflow" "github"
        instructions "Draft release notes from merged PRs."
        section "When to use" "Use when preparing a tagged release."
        template "Run {{{tool Bash}}} to inspect the git log."
    }

let output = AgentWriter.renderSkill mySkill (fun _ -> ())
// ---
// name: git-release
// description: Create consistent releases and changelogs. Use when preparing a tagged release.
// license: MIT
// compatibility: opencode
// metadata:
//   author: me
//   workflow: github
// ---
//
// ## Instructions
// Draft release notes from merged PRs.
// ...
```

## Type Definition

**New file: `src/FsAgent/Skill.fs`** — namespace `FsAgent.Skills`

```fsharp
namespace FsAgent.Skills

open FsAgent.AST
open FsAgent.Prompts

type Skill = {
    Name: string
    Description: string
    License: string option
    Compatibility: string option
    Metadata: Map<string, string>
    Sections: Node list
}

module Skill =
    let empty : Skill = {
        Name = ""
        Description = ""
        License = None
        Compatibility = None
        Metadata = Map.empty
        Sections = []
    }

[<AutoOpen>]
module SkillBuilder =
    type SkillBuilder() =
        member _.Yield _ = Skill.empty
        member _.Zero() = Skill.empty
        member _.Run(s) = s

        [<CustomOperation("name")>]
        member _.Name(s, value: string) = { s with Name = value }

        [<CustomOperation("description")>]
        member _.Description(s, value: string) = { s with Description = value }

        [<CustomOperation("license")>]
        member _.License(s, value: string) = { s with License = Some value }

        [<CustomOperation("compatibility")>]
        member _.Compatibility(s, value: string) = { s with Compatibility = Some value }

        [<CustomOperation("meta")>]
        member _.Meta(s, key: string, value: string) =
            { s with Metadata = s.Metadata |> Map.add key value }

        [<CustomOperation("role")>]
        member _.Role(s, text: string) =
            { s with Sections = s.Sections @ [Prompt.role text] }

        [<CustomOperation("objective")>]
        member _.Objective(s, text: string) =
            { s with Sections = s.Sections @ [Prompt.objective text] }

        [<CustomOperation("instructions")>]
        member _.Instructions(s, text: string) =
            { s with Sections = s.Sections @ [Prompt.instructions text] }

        [<CustomOperation("context")>]
        member _.Context(s, text: string) =
            { s with Sections = s.Sections @ [Prompt.context text] }

        [<CustomOperation("output")>]
        member _.Output(s, text: string) =
            { s with Sections = s.Sections @ [Prompt.output text] }

        [<CustomOperation("section")>]
        member _.Section(s, name: string, content: string) =
            { s with Sections = s.Sections @ [Section(name, [Text content])] }

        [<CustomOperation("import")>]
        member _.Import(s, path: string) =
            { s with Sections = s.Sections @ [AST.importRef path] }

        [<CustomOperation("importRaw")>]
        member _.ImportRaw(s, path: string) =
            { s with Sections = s.Sections @ [AST.importRawRef path] }

        [<CustomOperation("template")>]
        member _.Template(s, text: string) =
            { s with Sections = s.Sections @ [Template text] }

        [<CustomOperation("templateFile")>]
        member _.TemplateFile(s, path: string) =
            { s with Sections = s.Sections @ [TemplateFile path] }

        [<CustomOperation("examples")>]
        member _.Examples(s, examples: Node list) =
            { s with Sections = s.Sections @ [Prompt.examples examples] }

        [<CustomOperation("prompt")>]
        member _.Prompt(s, p: Prompt) =
            { s with Sections = s.Sections @ p.Sections }

    let skill = SkillBuilder()
```

## Writer Extension

Add `renderSkill` to the `AgentWriter` module in `Writers.fs`. Add `open FsAgent.Skills` alongside existing opens at the top of the file.

```fsharp
let renderSkill (s: Skill) (configure: Options -> unit) : string =
    let opts = defaultOptions()
    configure opts

    // Build frontmatter manually — Skill has fields not present on Agent
    let sb = System.Text.StringBuilder()
    sb.AppendLine("---") |> ignore
    sb.AppendLine($"name: {s.Name}") |> ignore
    sb.AppendLine($"description: {s.Description}") |> ignore
    s.License |> Option.iter (fun v -> sb.AppendLine($"license: {v}") |> ignore)
    s.Compatibility |> Option.iter (fun v -> sb.AppendLine($"compatibility: {v}") |> ignore)
    if not s.Metadata.IsEmpty then
        sb.AppendLine("metadata:") |> ignore
        s.Metadata |> Map.iter (fun k v -> sb.AppendLine($"  {k}: {v}") |> ignore)
    sb.AppendLine("---") |> ignore

    // Render sections using existing Markdown rendering, suppressing frontmatter block
    let agentLike = { Frontmatter = Map.empty; Sections = s.Sections }
    let ctx = {
        Format = opts.OutputFormat
        OutputType = Md
        Timestamp = DateTime.Now
        AgentName = Some s.Name
        AgentDescription = Some s.Description
    }
    let body = renderMd agentLike { opts with IncludeFrontmatter = false } ctx

    sb.ToString() + body
```

`OutputType` is forced to `Md`. The `configure` lambda can set `OutputFormat` for harness-aware template resolution; it defaults to `Opencode` via `defaultOptions()`.

## Library.fs Backward Compatibility Layer

```fsharp
type Skill = Skills.Skill

module DSL =
    // existing entries...
    let skill = Skills.SkillBuilder.skill
```

## Compilation Order (`FsAgent.fsproj`)

Insert `Skill.fs` after `Command.fs`:

```xml
<Compile Include="Command.fs" />
<Compile Include="Skill.fs" />      <!-- NEW -->
<Compile Include="Writers.fs" />
```

## Tests

**New file: `tests/FsAgent.Tests/SkillTests.fs`** (category A)

| Test | Description |
|------|-------------|
| `name appears in frontmatter` | `name: my-skill` in output |
| `description appears in frontmatter` | `description: ...` in output |
| `license omitted when not set` | No `license:` line when `None` |
| `license present when set` | `license: MIT` in output |
| `compatibility omitted when not set` | No `compatibility:` line |
| `compatibility present when set` | `compatibility: opencode` in output |
| `metadata block emitted correctly` | `metadata:\n  key: val` in output |
| `metadata omitted when empty` | No `metadata:` line when map is empty |
| `sections render as markdown` | `instructions`, `section` emit headings + content |
| `template resolves with harness` | `{{{tool Bash}}}` resolves via `opts.OutputFormat` |
| `import embeds file` | `import` node works as in agent/command |
| `prompt composable into skill` | Sections from `Prompt` are merged |
| `minimal skill renders name and description only` | Only two frontmatter lines in output |

## Files to Create / Modify

| File | Action | Notes |
|------|--------|-------|
| `src/FsAgent/Skill.fs` | **Create** | `Skill` type + `SkillBuilder` CE |
| `src/FsAgent/FsAgent.fsproj` | **Modify** | Add `Skill.fs` compile entry after `Command.fs` |
| `src/FsAgent/Writers.fs` | **Modify** | Add `open FsAgent.Skills`; add `renderSkill` to `AgentWriter` |
| `src/FsAgent/Library.fs` | **Modify** | Add `Skill` alias; add `skill` to `DSL` module |
| `tests/FsAgent.Tests/SkillTests.fs` | **Create** | Acceptance tests |
| `tests/FsAgent.Tests/FsAgent.Tests.fsproj` | **Modify** | Add `SkillTests.fs` compile entry |
| `ARCHITECTURE.md` | **Modify** | Add `Skill.fs`/`renderSkill` to namespace map and writer section |
| `CHANGELOG.md` | **Modify** | Entry under `[Unreleased]` |

## Open Questions

None — all design decisions confirmed.
