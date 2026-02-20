## Context

FsAgent follows a stratified design: DSL → AST (Node) → Writer → output. `Agent` and `Prompt` both have dedicated CE builders and writer functions. Slash commands are structurally a named prompt with a `description`-only frontmatter; the `name` field drives output file naming but is not part of the emitted content.

The existing writer pipeline (`renderAgent`, `renderPrompt`) uses an internal `private renderMd` function that accepts a synthetic `Agent` value. `WriterContext` is constructed internally — callers never build it. `Options` is a mutable record configured via a lambda.

Current state: no `Command.fs`, no `renderCommand`, no `SlashCommand` type.

## Goals / Non-Goals

**Goals:**
- Add `SlashCommand` record and `command { }` CE in a new `Command.fs` module.
- Add `AgentWriter.renderCommand` that renders `description`-only frontmatter + sections.
- Re-export from `Library.fs` for top-level access.
- Mirror the `Prompt` builder's section operations for API consistency.
- Zero changes to `Agent`, `Prompt`, `AST`, or existing writer logic.

**Non-Goals:**
- Validation of `Name` (kebab-case, non-empty) — deferred to a future cross-cutting validation change.
- Per-harness conditional output — all three harnesses emit identical Markdown today.
- JSON/YAML output types for commands — `renderCommand` exposes them for parity but no custom rendering is needed.

## Decisions

### 1. Dedicated type, not a wrapper around `Agent`

`SlashCommand` is its own record `{ Name: string; Description: string; Sections: Node list }`. The alternative — wrapping `Agent` — would inherit unused frontmatter fields and conflate file-naming metadata (`Name`) with agent identity. A dedicated type keeps the domain model precise and the CE surface minimal.

### 2. `Name` stored in `SlashCommand` but omitted from frontmatter

Slash command files are named by convention (`<name>.md`). The `name` is passed through to `WriterContext.AgentName` (for footer/template use) but not written into the YAML front block. This matches the observed `.opencode/commands/*.md` format where only `description` appears in frontmatter.

### 3. Writer delegates to `renderMd` via a synthetic `Agent`

`renderCommand` constructs a synthetic `Agent` with `Frontmatter = Map.ofList ["description", fmStr cmd.Description]` and `Sections = cmd.Sections`, then calls the private `renderMd` (via the same internal path as `renderAgent`). No new rendering logic is needed.

Alternative considered: add a new private `renderCommandMd` function. Rejected — it would duplicate all section/template/import rendering that `renderMd` already handles correctly.

### 4. `CommandBuilder` mirrors `PromptBuilder` operations

Operations: `name`, `description`, `role`, `objective`, `instructions`, `context`, `output`, `section`, `import`, `importRaw`, `template`, `templateFile`, `examples`, `prompt`. The `prompt` operation merges a `Prompt`'s sections into the command, enabling reuse of shared prompt definitions. This mirrors how `AgentBuilder.Prompt` works.

### 5. Module placement: `Command.fs` after `Agent.fs` in compile order

`Command.fs` depends on `AST` and `Prompts` (for `Prompt.role` etc.). It must precede `Writers.fs` so `renderCommand` can reference `SlashCommand`. The `.fsproj` compile order: `Tools.fs → AST.fs → Prompt.fs → Agent.fs → Command.fs → Writers.fs → Library.fs`.

### 6. `Library.fs` re-exports via type alias and `DSL` module

Consistent with existing pattern: `type SlashCommand = Commands.SlashCommand` at namespace level; `let command = Commands.CommandBuilder.command` inside `module DSL`. This keeps `open FsAgent` sufficient for most users.

## Risks / Trade-offs

- **`renderMd` is private** — `renderCommand` must live in the same module (`Writers.fs`) or the synthetic-Agent approach must be used from within the module. Since `renderCommand` is added directly to `AgentWriter` in `Writers.fs`, it has access to `renderMd`. No visibility issue.
- **`WriterContext` constructed internally** — `renderCommand` must build its own `ctx` with `AgentName = Some cmd.Name` and `AgentDescription = Some cmd.Description`. This is a one-time copy of the pattern in `renderAgent`; any future refactor to extract context-building should include `renderCommand`.
- **No harness differentiation** — if harnesses diverge in future, `renderCommand` will need the same conditional branching added to `renderMd`. Low risk now; isolated to one function.

## Open Questions

None — all design questions from the plan are resolved.
