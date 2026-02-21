# Harness-Aware File Writer

**Status:** Draft

## Problem

`renderAgent`, `renderSkill`, and `renderCommand` all return `string`. Callers are responsible for writing the result to disk at the correct path. The correct path depends on:

- Which harness (Opencode, Copilot, ClaudeCode)
- What artifact type (agent, skill, slash command)
- Whether the target is project-scope or global-scope

These conventions are non-trivial, differ per harness, and are already implicit in the library. Forcing every caller to re-implement them is unnecessary friction.

## Goal

Add a thin I/O layer that resolves the correct output path and writes the file, given the harness, artifact type, scope, and a root directory. Existing `render*` functions stay unchanged.

---

## File Path Conventions

### Opencode

| Artifact | Project path | Global path |
|---|---|---|
| Agent | `.opencode/agents/<name>.md` | `~/.config/opencode/agents/<name>.md` |
| Slash command | `.opencode/commands/<name>.md` | `~/.config/opencode/commands/<name>.md` |
| Skill | `.opencode/skills/<name>/SKILL.md` | `~/.config/opencode/skills/<name>/SKILL.md` |

- Agent identity = **file name** (no `name:` in frontmatter for Opencode agents).

### GitHub Copilot

| Artifact | Project path | Global path |
|---|---|---|
| Agent | `.github/agents/<name>.md` | n/a (org/enterprise managed) |
| Prompt/command | `.github/prompts/<name>.prompt.md` | n/a |
| Skill | `.github/skills/<name>/SKILL.md` | n/a |

- Agent identity = `name:` in frontmatter; file name is arbitrary but should match for clarity.
- Command files must use the `.prompt.md` suffix.

### ClaudeCode

| Artifact | Project path | Global path |
|---|---|---|
| Agent | `.claude/agents/<name>.md` | `~/.claude/agents/<name>.md` |
| Slash command | `.claude/commands/<name>.md` | `~/.claude/commands/<name>.md` |
| Slash command (namespaced) | `.claude/commands/<namespace>/<name>.md` | `~/.claude/commands/<namespace>/<name>.md` |
| Skill | `.claude/skills/<name>/SKILL.md` | `~/.claude/skills/<name>/SKILL.md` |

---

## API Design

### Scope discriminated union

```fsharp
type WriteScope =
    | Project of rootDir: string   // relative to given directory
    | Global                       // relative to OS user config root
```

### Artifact type discriminated union

```fsharp
type ArtifactKind =
    | AgentArtifact
    | CommandArtifact of namespace_: string option   // namespace for ClaudeCode
    | SkillArtifact
```

### Core function signature

```fsharp
// Resolve the output path without writing
val resolveOutputPath :
    harness: AgentHarness ->
    kind: ArtifactKind ->
    name: string ->
    scope: WriteScope ->
    string

// Write content to the resolved path, creating directories as needed
// Returns the path written to
val writeFile :
    harness: AgentHarness ->
    kind: ArtifactKind ->
    name: string ->
    scope: WriteScope ->
    content: string ->
    string

// Convenience: render agent and write to disk
val writeAgent :
    agent: Agent ->
    harness: AgentHarness ->
    scope: WriteScope ->
    configure: (Options -> unit) ->
    string   // returns resolved path

// Convenience: render skill and write to disk
val writeSkill :
    skill: Skill ->
    harness: AgentHarness ->
    scope: WriteScope ->
    configure: (Options -> unit) ->
    string

// Convenience: render command and write to disk
val writeCommand :
    cmd: SlashCommand ->
    harness: AgentHarness ->
    scope: WriteScope ->
    namespace_: string option ->   // only used by ClaudeCode
    configure: (Options -> unit) ->
    string
```

### Name extraction

The `name` argument to `writeAgent`/`writeSkill`/`writeCommand` is derived from:

- `Agent`: `agent.Frontmatter["name"]` (if present) — Opencode agents use the file name, so if `name` is absent in frontmatter the caller must supply it explicitly, or a separate overload accepts an explicit name parameter.
- `SlashCommand`: `cmd.Name` (always present after validation).
- `Skill`: `skill.Frontmatter["name"]` (required, validated before render).

Fallback: raise `ArgumentException` if name cannot be determined.

---

## Global Path Resolution

| Platform | Base path |
|---|---|
| Linux/macOS | `~/.config/` for Opencode; `~/.claude/` for ClaudeCode |
| Windows | `%APPDATA%\` for Opencode; `%USERPROFILE%\.claude\` for ClaudeCode |

Use `System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)` and `Environment.SpecialFolder.ApplicationData` — no new dependencies.

Copilot does not have a global file-system path; calling `writeAgent` or `writeCommand` with `Global` scope on `Copilot` harness should raise `NotSupportedException` with a clear message.

---

## Implementation Notes

- **Module placement**: New `FileWriter.fs` file added after `Writers.fs` in `FsAgent.fsproj`. Namespace: `FsAgent.Writers`.
- **No breaking changes**: All existing `render*` functions are untouched.
- **Directory creation**: Use `Directory.CreateDirectory` (idempotent) before writing.
- **Re-exports**: Add `writeAgent`, `writeSkill`, `writeCommand`, `resolveOutputPath` to `Library.fs` DSL re-exports.
- **Pure path resolution**: `resolveOutputPath` is pure (no I/O) and separately testable.

---

## Design Constraints

- DSL layer must not depend on output formats — this module lives in the Writers layer, not DSL.
- Writers must not mutate AST — file writer only calls `render*` and writes the result.
- Keep render functions pure; all I/O is in `FileWriter`.

---

## Example Usage

```fsharp
open FsAgent
open FsAgent.Writers

let myAgent =
    agent {
        name "my-assistant"
        description "A helpful assistant"
        tools [Bash; Read; Write]
    }

// Write to project-local Opencode path: .opencode/agents/my-assistant.md
let path =
    writeAgent myAgent AgentHarness.Opencode (Project ".") (fun opts ->
        opts.OutputFormat <- AgentHarness.Opencode)

// Write skill globally for ClaudeCode: ~/.claude/skills/my-skill/SKILL.md
let skillPath =
    writeSkill mySkill AgentHarness.ClaudeCode Global id

// Resolve path without writing
let resolved =
    resolveOutputPath AgentHarness.Copilot CommandArtifact None "deploy" (Project ".")
// → ".github/prompts/deploy.prompt.md"
```

---

## Open Questions

1. Should `writeAgent` for Opencode accept an explicit `name` parameter to override frontmatter (since Opencode uses file name as identity, not `name:` field)?
2. Should there be a multi-harness write helper (e.g. `writeAgentToAll`) for users who want to deploy to all harnesses at once?
3. For Copilot with `Global` scope — raise or silently skip?
