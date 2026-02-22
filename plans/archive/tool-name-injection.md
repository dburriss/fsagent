# Plan: Harness-Aware Tool Name Injection in Prompt Templates

Status: Done

## Problem

Prompt text often references tool names inline (e.g., "use the `bash` tool to run
commands"). These names differ per harness:

| Tool DU   | Opencode  | ClaudeCode              | Copilot |
|-----------|-----------|-------------------------|---------|
| Bash      | bash      | Bash                    | bash    |
| Read      | read      | Read                    | read    |
| Glob      | grep      | Glob                    | search  |
| TodoWrite | todowrite | TaskCreate, TaskUpdate  | todo    |

There is no current mechanism to write harness-agnostic prompt text that resolves
tool names correctly at write time.

## Solution

Extend the `Template` module to support a Fue template function `tool` that
resolves a `Tool` DU case name to its harness-correct string(s) at write time.
The writer injects this function automatically based on the current `OutputFormat`.

No new AST nodes. No new DSL operations. Purely a rendering-layer concern.

## User-Facing Syntax

```fsharp
prompt {
    template "Use the {{{tool Bash}}} tool to run shell commands."
    template "Mark tasks done using {{{tool TodoWrite}}}."
}
```

Rendered for Opencode:   `Use the bash tool to run shell commands.`
Rendered for ClaudeCode: `Use the Bash tool to run shell commands.`
Rendered for ClaudeCode: `Mark tasks done using TaskCreate, TaskUpdate.`
Rendered for Copilot:    `Mark tasks done using todo.`

Unknown tool name (e.g. `{{{tool Foo}}}`) passes through via `Tool.Custom "Foo"`,
returning `"Foo"` unchanged for all harnesses — consistent with existing `Custom`
behaviour in `toolToString`.

## Key Design Decision: Fue Function Binding

Fue's `add` accepts `string -> 'a`. We bind the key `"tool"` to a function
`(string -> string)` that:

1. Looks up the argument in a `Map<string, Tool>` (e.g. `"Bash" -> Tool.Bash`)
2. Falls back to `Tool.Custom name` for unrecognised names (pass-through)
3. Calls the existing `toolToString harness tool -> string list`
4. Joins with `", "` for multi-name results

This keeps the template syntax as `{{{tool Bash}}}` — a Fue function call with one
argument.

## Architecture

Templates are rendered at write time in `renderMd` (`Writers.fs` lines 382–387),
where `opts.OutputFormat` (the `AgentHarness`) is already in scope. This is the
only place that needs to change.

```
DSL: template "Use {{{tool Bash}}}"
        ↓  (stored as Template node in AST — no change)
renderMd: Template text branch
        ↓  (currently: Template.renderInline text opts.TemplateVariables)
        ↓  (NEW:       Template.renderWithHarness text opts.TemplateVariables harness toolToString)
Template.renderWithHarness: injects `tool` Fue function into data context
        ↓
Rendered string: "Use bash"
```

## Files to Modify

| File | Change |
|------|--------|
| `src/FsAgent/Writers.fs` | 1. Add `toolNameMap` (DU case name string → `Tool` value); 2. Add `renderWithHarness` and `renderFileWithHarness` to `Template` module; 3. Update `renderMd` `Template`/`TemplateFile` branches to call harness-aware render |
| `tests/FsAgent.Tests/AgentWriterTests.fs` | Acceptance tests (see below) |
| `CHANGELOG.md` | Entry under Unreleased |

`toolToString` is already defined in the same module — no visibility change needed.

## Implementation Steps

### Step 1: Tool name lookup map

Add inside `AgentWriter` module, before `renderMd`, after `toolToString`:

```fsharp
let private toolNameMap : Map<string, Tool> =
    Map.ofList [
        "Write",     Tool.Write
        "Edit",      Tool.Edit
        "Bash",      Tool.Bash
        "Shell",     Tool.Shell
        "Read",      Tool.Read
        "Glob",      Tool.Glob
        "List",      Tool.List
        "LSP",       Tool.LSP
        "Skill",     Tool.Skill
        "TodoWrite", Tool.TodoWrite
        "TodoRead",  Tool.TodoRead
        "WebFetch",  Tool.WebFetch
        "WebSearch", Tool.WebSearch
        "Question",  Tool.Question
        "Todo",      Tool.Todo
    ]
```

`Tool.Custom s` is not in the map — unknown names fall through to the Custom
pass-through branch below.

### Step 2: Add `renderWithHarness` and `renderFileWithHarness` to `Template` module

These sit alongside the existing `renderInline` / `renderFile` functions. Because
`toolToString` is defined later in `AgentWriter`, it is passed in as a parameter
to avoid a forward-dependency within the same file.

```fsharp
let renderWithHarness
        (text: string)
        (variables: TemplateVariables)
        (harness: AgentHarness)
        (toolResolver: AgentHarness -> Tool -> string list)
        (nameMap: Map<string, Tool>) : string =
    try
        let toolFn (name: string) : string =
            let tool =
                match nameMap |> Map.tryFind name with
                | Some t -> t
                | None   -> Tool.Custom name   // pass-through for unknown names
            toolResolver harness tool |> String.concat ", "
        let data =
            variables
            |> Map.fold (fun acc key value -> acc |> add key value) init
            |> add "tool" toolFn
        data |> fromText text
    with
    | ex -> $"[Template error: {ex.Message}]"

let renderFileWithHarness
        (path: string)
        (variables: TemplateVariables)
        (harness: AgentHarness)
        (toolResolver: AgentHarness -> Tool -> string list)
        (nameMap: Map<string, Tool>) : string =
    try
        if not (System.IO.File.Exists(path)) then
            $"[Template file not found: {path}]"
        else
            let toolFn (name: string) : string =
                let tool =
                    match nameMap |> Map.tryFind name with
                    | Some t -> t
                    | None   -> Tool.Custom name
                toolResolver harness tool |> String.concat ", "
            let data =
                variables
                |> Map.fold (fun acc key value -> acc |> add key value) init
                |> add "tool" toolFn
            data |> fromFile path
    with
    | ex -> $"[Template error: {ex.Message}]"
```

Note: `AgentHarness` and `Tool` are referenced here but defined later in the same
file. `AgentHarness` will be moved above the `Template` module (see Forward-Dependency
Resolution below) to make these types available.

### Step 3: Update `renderMd` Template/TemplateFile branches

```fsharp
// Before
| Template text ->
    let rendered = Template.renderInline text opts.TemplateVariables
    sb.AppendLine(rendered) |> ignore
| TemplateFile path ->
    let rendered = Template.renderFile path opts.TemplateVariables
    sb.AppendLine(rendered) |> ignore

// After
| Template text ->
    let rendered = Template.renderWithHarness text opts.TemplateVariables ctx.Format toolToString toolNameMap
    sb.AppendLine(rendered) |> ignore
| TemplateFile path ->
    let rendered = Template.renderFileWithHarness path opts.TemplateVariables ctx.Format toolToString toolNameMap
    sb.AppendLine(rendered) |> ignore
```

`ctx.Format` is the `AgentHarness` already in scope in `renderMd`.

## Tests (A — Acceptance)

All tests go in `AgentWriterTests.fs` and exercise the full DSL → write pipeline.

```fsharp
// 1. Bash resolves correctly per harness
[<Fact>]
let ``tool Bash resolves to bash for Opencode`` ()

[<Fact>]
let ``tool Bash resolves to Bash for ClaudeCode`` ()

[<Fact>]
let ``tool Bash resolves to bash for Copilot`` ()

// 2. Multi-name resolution
[<Fact>]
let ``tool TodoWrite resolves to TaskCreate, TaskUpdate for ClaudeCode`` ()

// 3. Glob harness differences
[<Fact>]
let ``tool Glob resolves to grep for Opencode`` ()

[<Fact>]
let ``tool Glob resolves to search for Copilot`` ()

// 4. Unknown name passes through
[<Fact>]
let ``unknown tool name passes through unchanged`` ()
// template "{{{tool my_mcp_tool}}}" → "my_mcp_tool" for all harnesses

// 5. Coexistence with TemplateVariables
[<Fact>]
let ``tool function coexists with user TemplateVariables`` ()
// template "Hello {{{name}}}, use {{{tool Read}}}"
// TemplateVariables = Map ["name", "world" :> obj]
// Opencode → "Hello world, use read"

// 6. Existing renderInline unchanged
[<Fact>]
let ``renderInline without harness is unaffected`` ()
// No regression — {{{tool X}}} in plain renderInline produces a Fue error or literal
```

## Forward-Dependency Resolution

`Template` module is compiled before `AgentWriter` in the same file. `AgentHarness`
and `Tool` types are needed by `renderWithHarness`.

**Decision**: Move the `AgentHarness` type definition above the `Template` module.
It has no dependencies on `Template` or anything else in `AgentWriter`, so this
is a safe, clean reorder with no cascading changes.
