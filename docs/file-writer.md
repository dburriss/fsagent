# Writing Files to Disk

FsAgent provides two APIs for writing agent artifacts to disk: a set of module-level functions for simple one-off writes, and the `AgentFileWriter` class for testable, injectable usage.

---

## Output path table

Paths are determined by the combination of **harness**, **artifact kind**, and **scope**.

### Project scope

| Harness | Artifact | Path |
|---------|----------|------|
| OpenCode | Agent | `.opencode/agents/<name>.md` |
| OpenCode | Skill | `.agents/skills/<name>/SKILL.md` |
| OpenCode | Command | `.opencode/commands/<name>.md` |
| Copilot | Agent | `.github/agents/<name>.md` |
| Copilot | Skill | `.github/skills/<name>/SKILL.md` |
| Copilot | Command | `.github/prompts/<name>.prompt.md` |
| ClaudeCode | Agent | `.claude/agents/<name>.md` |
| ClaudeCode | Skill | `.claude/skills/<name>/SKILL.md` |
| ClaudeCode | Command | `.claude/commands/<name>.md` |
| ClaudeCode | Command (namespaced) | `.claude/commands/<ns>/<name>.md` |

### Global scope

| Harness | Artifact | Path |
|---------|----------|------|
| OpenCode | Agent | `~/.config/opencode/agents/<name>.md` *(Unix)* |
| OpenCode | Skill | `~/.config/opencode/skills/<name>/SKILL.md` *(Unix)* |
| OpenCode | Command | `~/.config/opencode/commands/<name>.md` *(Unix)* |
| Copilot | All | See [Copilot global scope](#copilot-global-scope) |
| ClaudeCode | Agent | `~/.claude/agents/<name>.md` |
| ClaudeCode | Skill | `~/.claude/skills/<name>/SKILL.md` |
| ClaudeCode | Command | `~/.claude/commands/<name>.md` |

> **OpenCode Skill note:** OpenCode searches `.agents/skills/` (priority 5) and `.opencode/skills/` (priority 1). FsAgent writes skills to `.agents/skills/` by default for the project scope — this is the cross-tool Agent Skills spec path. Rename or copy if you need `.opencode/skills/` specifically.

---

## Module functions (low-level API)

```fsharp
open FsAgent.Writers.FileWriter
open FsAgent.Writers   // AgentHarness, WriteScope
```

### `writeFile`

Lowest-level function. Writes arbitrary content to the resolved path.

```fsharp
val writeFile :
    harness  : AgentHarness ->
    kind     : ArtifactKind ->
    name     : string ->
    scope    : WriteScope ->
    content  : string ->
    string   // returns the resolved path
```

### `writeAgent`

Renders an `Agent` and writes it.

```fsharp
val writeAgent :
    agent     : Agent ->
    harness   : AgentHarness ->
    scope     : WriteScope ->
    configure : (AgentWriter.Options -> unit) ->
    string
```

```fsharp
let path = writeAgent myAgent AgentHarness.Opencode (Project ".") (fun _ -> ())
```

### `writeSkill`

```fsharp
val writeSkill :
    skill     : Skill ->
    harness   : AgentHarness ->
    scope     : WriteScope ->
    configure : (AgentWriter.Options -> unit) ->
    string
```

### `writeCommand`

```fsharp
val writeCommand :
    cmd        : SlashCommand ->
    harness    : AgentHarness ->
    scope      : WriteScope ->
    namespace_ : string option ->    // ClaudeCode subdirectory; None for flat
    configure  : (AgentWriter.Options -> unit) ->
    string
```

---

## `AgentFileWriter` (injectable)

Preferred for testable code. Accepts an `IFileSystem` so tests can use `MockFileSystem`.

```fsharp
type AgentFileWriter(
    fileSystem   : IFileSystem,
    scope        : WriteScope,
    ?configure   : AgentWriter.Options -> unit,
    ?copilotRoot : string,
    ?folderVariant : FolderVariant)
```

### Members

| Member | Signature |
|--------|-----------|
| `WriteAgent` | `(agent, harness) -> string` |
| `WriteSkill` | `(skill, harness) -> string` |
| `WriteCommand` | `(cmd, harness, ?ns) -> string` |

```fsharp
open FsAgent.Writers
open Testably.Abstractions

// Production
let writer = AgentFileWriter(RealFileSystem(), Project ".")
let path = writer.WriteAgent(myAgent, AgentHarness.Opencode)

// Tests (using MockFileSystem from Testably.Abstractions.Testing)
open Testably.Abstractions.Testing
let fs = MockFileSystem()
let writer = AgentFileWriter(fs, Project "/repo")
let path = writer.WriteSkill(mySkill, AgentHarness.ClaudeCode)
Assert.True(fs.File.Exists(path))
```

---

## `resolveOutputPath` (pure, no I/O)

When you only need the path without writing:

```fsharp
val resolveOutputPath :
    harness : AgentHarness ->
    kind    : ArtifactKind ->
    name    : string ->
    scope   : WriteScope ->
    string
```

Defaults to `AgentsFolder` for OpenCode project-scope skills. For explicit control use `resolveOutputPathWith`.

```fsharp
let p = FileWriter.resolveOutputPath
            AgentHarness.Copilot
            AgentArtifact
            "my-agent"
            (Project "/repo")
// → "/repo/.github/agents/my-agent.md"
```

## `resolveOutputPathWith` (pure, no I/O)

Variant that accepts an explicit `FolderVariant` to control the project-scope skill root.
`ClaudeFolder` affects both OpenCode and Copilot; `AgentsFolder`/`OpencodeFolder` affect OpenCode only.

```fsharp
val resolveOutputPathWith :
    harness      : AgentHarness ->
    kind         : ArtifactKind ->
    name         : string ->
    scope        : WriteScope ->
    folderVariant: FolderVariant ->
    string
```

```fsharp
// Write to .opencode/skills/ instead of the default .agents/skills/
let p = FileWriter.resolveOutputPathWith
            AgentHarness.Opencode
            SkillArtifact
            "my-skill"
            (Project "/repo")
            OpencodeFolder
// → "/repo/.opencode/skills/my-skill/SKILL.md"

// Write to .claude/skills/ — readable by ClaudeCode, OpenCode, and Copilot
let p = FileWriter.resolveOutputPathWith
            AgentHarness.Copilot
            SkillArtifact
            "my-skill"
            (Project "/repo")
            ClaudeFolder
// → "/repo/.claude/skills/my-skill/SKILL.md"
```

---

## `ConfigPaths` module

Pure helpers for harness root directories. No I/O.

```fsharp
module ConfigPaths =
    val resolveGlobalRoot  : harness:AgentHarness -> copilotRoot:string option -> string
    val resolveProjectRoot : harness:AgentHarness -> rootDir:string -> string
```

---

## Copilot global scope

Copilot has no standard OS-level config path. Global agents live in an org's `.github-private` repository. Supply the local clone path in priority order:

1. `copilotRoot` parameter on `AgentFileWriter` constructor.
2. `COPILOT_GLOBAL_ROOT` environment variable.
3. `NotSupportedException` if neither is set.

```fsharp
// Via constructor
let writer = AgentFileWriter(RealFileSystem(), Global, copilotRoot = "/path/to/github-private")

// Via env var (set before process starts)
// COPILOT_GLOBAL_ROOT=/path/to/github-private
let writer = AgentFileWriter(RealFileSystem(), Global)
```

---

## `WriteScope`

```fsharp
type WriteScope =
    | Project of rootDir: string   // relative or absolute; resolved via Path.GetFullPath
    | Global
```

## `ArtifactKind`

```fsharp
type ArtifactKind =
    | AgentArtifact
    | SkillArtifact
    | CommandArtifact of namespace_: string option
```

## `FolderVariant`

Controls which root directory project-scope skills are written to.
`ClaudeFolder` affects both OpenCode and Copilot; `AgentsFolder`/`OpencodeFolder` affect OpenCode only.

```fsharp
type FolderVariant =
    | AgentsFolder   // default — cross-tool Agent Skills spec path (.agents/skills/)
    | OpencodeFolder // OpenCode-specific path (.opencode/skills/)
    | ClaudeFolder   // cross-tool Claude-compatible path (.claude/skills/); affects OpenCode and Copilot
```
