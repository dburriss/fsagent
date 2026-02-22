# File Writer — Path Resolution & `AgentFileWriter`

**Status:** Draft

---

## Description

Fill the gaps in `FileWriter.fs` identified in the backlog:

1. **OpenCode skill path** — current code sends project-scope OpenCode skills to `.opencode/skills/`; the spec default is `.agents/skills/` (cross-tool Agent Skills path). Support both via a new `OpenCodeSkillPath` option.
2. **Copilot global scope** — `resolveGlobalRoot` raises `NotSupportedException` unconditionally; implement `COPILOT_GLOBAL_ROOT` env-var fallback before raising.
3. **`ConfigPaths` module** — `resolveGlobalRoot` and `harnessProjectRoot` are private helpers; expose them as a public `ConfigPaths` module with `resolveGlobalRoot` and `resolveProjectRoot`.
4. **`AgentFileWriter` class** — no injectable class exists; implement it with `IFileSystem` from `Testably.Abstractions` to enable in-memory testing.

---

## Scope

### 1. `OpenCodeSkillPath` option

Add a DU to `FileWriter.fs`:

```fsharp
type OpenCodeSkillPath =
    | AgentsFolder       // .agents/skills/<name>/SKILL.md  (default, cross-tool spec)
    | OpencodeFolder     // .opencode/skills/<name>/SKILL.md
```

Thread this option through `resolveOutputPath` via an overload:

- Primary overload: `resolveOutputPath harness kind name scope opencodeSkillPath` (explicit)
- Backward-compat overload: `resolveOutputPath harness kind name scope` — defaults to `AgentsFolder`

Update `writeFile`, `writeSkill`, and `AgentFileWriter.WriteSkill` accordingly.

### 2. Copilot global scope via env var

Update `resolveGlobalRoot` (and `ConfigPaths.resolveGlobalRoot`):

```fsharp
| AgentHarness.Copilot ->
    match copilotRoot with
    | Some r -> r
    | None ->
        match Environment.GetEnvironmentVariable("COPILOT_GLOBAL_ROOT") with
        | s when not (String.IsNullOrEmpty s) -> s
        | _ -> raise (NotSupportedException "...")
```

### 3. `ConfigPaths` module

Extract from private helpers and expose as a public module in `FileWriter.fs`:

```fsharp
module ConfigPaths =
    val resolveGlobalRoot  : harness:AgentHarness -> copilotRoot:string option -> string
    val resolveProjectRoot : harness:AgentHarness -> rootDir:string -> string
```

`copilotRoot` takes priority over `COPILOT_GLOBAL_ROOT` env var, then raises. Internal helpers delegate to this module.

### 4. `AgentFileWriter` class

```fsharp
type AgentFileWriter(
    fileSystem   : IFileSystem,          // Testably.Abstractions
    scope        : WriteScope,
    ?configure   : AgentWriter.Options -> unit,
    ?copilotRoot : string,
    ?skillPath   : OpenCodeSkillPath)
```

Members mirror the module-level functions:
- `WriteAgent(agent, harness) -> string`
- `WriteSkill(skill, harness) -> string`
- `WriteCommand(cmd, harness, ?ns) -> string`

Uses `fileSystem.File.WriteAllText` and `fileSystem.Directory.CreateDirectory`. Path resolution delegates to the updated `resolveOutputPath`.

Requires adding `Testably.Abstractions` (and `Testably.Abstractions.Testing` in the test project) as NuGet dependencies.

Re-export `ConfigPaths` and `AgentFileWriter` from `Library.fs` so they are accessible via `open FsAgent`.

---

## Dependencies / Prerequisites

- `Testably.Abstractions` NuGet package must be added to `FsAgent` project.
- `Testably.Abstractions.Testing` must be added to the test project.
- No AST or DSL changes required.

---

## Impact on Existing Code

| Area | Impact |
|------|--------|
| `resolveOutputPath` signature | New overload added; existing callers use the arity-4 overload (unchanged). |
| Test `A 7.3` | Currently asserts `.opencode/skills/` for OpenCode — must be updated to `.agents/skills/` (new default). |
| Test `A 7.4` | Global OpenCode skill path assertion needs same update. |
| `writeFile` / `writeSkill` | Pass-through for new option; callers using `writeSkill` unaffected (default `AgentsFolder` applies). |
| `Library.fs` re-exports | Add `ConfigPaths` and `AgentFileWriter` to `FsAgent` namespace re-exports. |

---

## Acceptance Criteria

- [ ] `resolveOutputPath Opencode SkillArtifact "x" (Project "/r")` → `/r/.agents/skills/x/SKILL.md` (default overload)
- [ ] `resolveOutputPath Opencode SkillArtifact "x" (Project "/r") OpencodeFolder` → `/r/.opencode/skills/x/SKILL.md`
- [ ] `ConfigPaths.resolveGlobalRoot Copilot (Some "/path/to/private")` → `"/path/to/private"`
- [ ] `ConfigPaths.resolveGlobalRoot Copilot None` with `COPILOT_GLOBAL_ROOT` set → env var value
- [ ] `ConfigPaths.resolveGlobalRoot Copilot None` with no env var → `NotSupportedException`
- [ ] `ConfigPaths.resolveProjectRoot Opencode "/repo"` → `"/repo/.opencode"`
- [ ] `AgentFileWriter` with `MockFileSystem` writes to correct path without touching real disk
- [ ] `AgentFileWriter` `copilotRoot` constructor param takes priority over env var for Copilot global scope
- [ ] `ConfigPaths` and `AgentFileWriter` accessible via `open FsAgent`
- [ ] Existing tests (A 7.x, C 8.x) pass after signature and path updates

---

## Testing Strategy

**A-category (pure, no I/O):**
- Update `A 7.3` / `A 7.4` for new OpenCode skill default path (`.agents/skills/`).
- Add tests for `AgentsFolder` vs `OpencodeFolder` explicit option.
- Add `ConfigPaths` unit tests: all harnesses × project/global scope, Copilot global with override vs env var vs raise.

**C-category (filesystem boundary):**
- Add `MockFileSystem` tests for `AgentFileWriter`: each artifact type × harness, verifying file exists at resolved path.
- Add test for `AgentFileWriter` Copilot global with `copilotRoot` constructor param.

---

## Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| OpenCode skill path change is a behavioral breaking change for existing callers using default | Document in CHANGELOG; the overload preserves call-site arity but changes the output path for OpenCode skills. |
| `Testably.Abstractions` version conflict | Pin to a stable version; check for existing transitive references. |
| Copilot env-var may conflict in CI environments | `COPILOT_GLOBAL_ROOT` is opt-in, not set by default; document clearly in README. |
