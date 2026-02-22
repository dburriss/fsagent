# Plan: Field Validation for Command and Skill Builders

Status: Done

## Overview

Add writer-time validation to `renderCommand` and `renderSkill` so that missing or
blank required fields fail fast with a clear error, consistent with the existing
`renderAgent` Copilot guard in `Writers.fs:496–502`.

All validation errors are collected and reported in a single exception — the user
sees every problem at once rather than fixing one field at a time.

No changes to DSL types, builders, or AST. No breaking changes to valid call sites.

## Scope

### Required fields

| Builder | Field | Where stored | Rule |
|---|---|---|---|
| `CommandBuilder` | `name` | `SlashCommand.Name` (record field) | Non-empty, non-whitespace |
| `CommandBuilder` | `description` | `SlashCommand.Description` (record field) | Non-empty, non-whitespace |
| `SkillBuilder` | `name` | `Skill.Frontmatter["name"]` (map entry) | Present and non-empty/non-whitespace |
| `SkillBuilder` | `description` | `Skill.Frontmatter["description"]` (map entry) | Present and non-empty/non-whitespace |

Out of scope: `temperature` range, `version` format, `model` non-empty — deferred.

## Design Decisions

- **Writer-time validation** — mirrors the `renderAgent` Copilot pattern; validation
  runs at the start of `renderCommand` / `renderSkill` before any serialization.
- **Collect-then-throw** — all violations are gathered into a `string list` before
  raising; a single `failwith` emits every problem in one message. Sequential
  `failwith` guards (fail-fast) are explicitly avoided.
- **Bulleted error format** — message header followed by one `- <error>` line per
  violation, e.g.:
  ```
  SlashCommand validation failed:
  - requires a non-empty 'name'
  - requires a non-empty 'description'
  ```
- **`failwith` on violation** — same error mechanism as existing validation; no
  `Result` wrapper.
- **`Skill` stays map-based** — no structural change to `Skill.fs`; migrate to typed
  record is a separate concern tracked in `skill-dsl.md`.
- **All harnesses enforced** — `renderCommand` has no harness-conditional path today;
  validation applies unconditionally. `renderSkill` similarly applies regardless of
  `AgentHarness` argument.

## Changes

### `src/FsAgent/Writers.fs`

Replace the sequential guards in `renderAgent` (Copilot block) with collect pattern:

```fsharp
match opts.OutputFormat with
| Copilot ->
    let name = agent.Frontmatter.TryFind "name" |> Option.map string
    let desc = agent.Frontmatter.TryFind "description" |> Option.map string
    let errors = [
        if name.IsNone then "requires 'name' in frontmatter"
        if desc.IsNone then "requires 'description' in frontmatter"
    ]
    if not errors.IsEmpty then
        failwith ("Agent (Copilot) validation failed:\n" + (errors |> List.map (sprintf "- %s") |> String.concat "\n"))
| _ -> ()
```

Replace the sequential guards in `renderCommand` with collect pattern:

```fsharp
// Validation
let errors = [
    if cmd.Name |> System.String.IsNullOrWhiteSpace then "requires a non-empty 'name'"
    if cmd.Description |> System.String.IsNullOrWhiteSpace then "requires a non-empty 'description'"
]
if not errors.IsEmpty then
    failwith ("SlashCommand validation failed:\n" + (errors |> List.map (sprintf "- %s") |> String.concat "\n"))
```

Replace the sequential guards in `renderSkill` with collect pattern:

```fsharp
// Validation
let skillName = skill.Frontmatter.TryFind "name" |> Option.map string
let skillDesc = skill.Frontmatter.TryFind "description" |> Option.map string
let errors = [
    if skillName |> Option.forall System.String.IsNullOrWhiteSpace then "requires a non-empty 'name' in frontmatter"
    if skillDesc |> Option.forall System.String.IsNullOrWhiteSpace then "requires a non-empty 'description' in frontmatter"
]
if not errors.IsEmpty then
    failwith ("Skill validation failed:\n" + (errors |> List.map (sprintf "- %s") |> String.concat "\n"))
```

### `tests/FsAgent.Tests/CommandTests.fs`

Existing single-field C tests remain valid — a single violation still throws.
Add one new test asserting both field names appear when both are missing:

```fsharp
[<Fact>]
let ``C: renderCommand reports all errors when name and description are both empty`` () =
    let cmd : SlashCommand = { Name = ""; Description = ""; Sections = [] }
    let ex = Assert.Throws<System.Exception>(fun () ->
        AgentWriter.renderCommand cmd (fun _ -> ()) |> ignore)
    Assert.Contains("'name'", ex.Message)
    Assert.Contains("'description'", ex.Message)
```

### `tests/FsAgent.Tests/SkillTests.fs`

Existing single-field C tests remain valid.
Add one new test asserting both field names appear when both are missing:

```fsharp
[<Fact>]
let ``C: renderSkill reports all errors when name and description are both missing`` () =
    let s : Skill = { Frontmatter = Map.empty; Sections = [] }
    let ex = Assert.Throws<System.Exception>(fun () ->
        AgentWriter.renderSkill s AgentWriter.Opencode (fun _ -> ()) |> ignore)
    Assert.Contains("'name'", ex.Message)
    Assert.Contains("'description'", ex.Message)
```

## Files to Modify

| File | Change |
|---|---|
| `src/FsAgent/Writers.fs` | Replace sequential guards with collect-then-throw in `renderAgent`, `renderCommand`, `renderSkill` |
| `tests/FsAgent.Tests/CommandTests.fs` | Add 1 C test for both-fields-missing case |
| `tests/FsAgent.Tests/SkillTests.fs` | Add 1 C test for both-fields-missing case |
| `CHANGELOG.md` | Note under `[Unreleased]` |

## Steps

1. Update `renderAgent` validation in `Writers.fs`
2. Update `renderCommand` validation in `Writers.fs`
3. Update `renderSkill` validation in `Writers.fs`
4. Add both-fields-missing test to `CommandTests.fs`
5. Add both-fields-missing test to `SkillTests.fs`
6. Run `dotnet build && dotnet test` — verify clean

## Open Questions

None.
