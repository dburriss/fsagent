## Context

`FileWriter.fs` provides path resolution and file writing for agent artifacts across three harnesses (Opencode, Copilot, ClaudeCode). The current implementation has four gaps: OpenCode project-scope skills are incorrectly routed to `.opencode/skills/` instead of the cross-tool Agent Skills spec path `.agents/skills/`; Copilot global scope always raises without checking `COPILOT_GLOBAL_ROOT`; all path-resolution logic is private, blocking consumers who only need paths without I/O; and there is no injectable class, so tests must use real filesystem temp directories.

The existing `harness-aware-file-writer` spec covers the current API shape; this change extends it.

## Goals / Non-Goals

**Goals:**
- Correct the OpenCode project-scope skill default path to `.agents/skills/` while preserving `.opencode/skills/` as an opt-in
- Implement Copilot global scope via `COPILOT_GLOBAL_ROOT` env var before raising
- Expose a public `ConfigPaths` module for pure path helpers
- Implement `AgentFileWriter` class with `IFileSystem` injection for in-memory testing
- Re-export new public surfaces from `Library.fs`

**Non-Goals:**
- Changes to `AgentWriter`, `Writers.fs`, or any AST/DSL layer
- Adding harnesses beyond Opencode, Copilot, ClaudeCode
- Windows-specific global path variants (existing behaviour preserved)
- `AgentReader` or any read-path work

## Decisions

### D1: `FolderVariant` DU, not a bool or string

**Decision**: Introduce `type FolderVariant = AgentsFolder | OpencodeFolder | ClaudeFolder`.

**Rationale**: A named DU is self-documenting at call sites and extensible without signature changes. A bool (`useAgentsFolder`) would be confusing; a string would be unvalidated. The DU defaults to `AgentsFolder` in the arity-4 overload, making the breaking path change the new default while keeping old callers syntactically valid. `ClaudeFolder` extends the DU to cover the cross-tool `.claude/skills/` path supported by all three harnesses; it also affects Copilot (unlike `AgentsFolder`/`OpencodeFolder`), which is intentional since Copilot supports `.claude/skills/`.

**Alternative considered**: An optional parameter `?skillPath` — rejected because F# optional parameters on module functions are less idiomatic than overloads and complicate the arity-5 explicit form.

### D2: Two overloads of `resolveOutputPath`

**Decision**: Keep the existing arity-4 signature (`harness kind name scope`) as the default overload (uses `AgentsFolder`), and add arity-5 (`harness kind name scope folderVariant`) as the explicit form.

**Rationale**: Preserves call-site compatibility for all existing callers. The behavioral change (new default path) is intentional and documented in CHANGELOG; the signature itself does not break.

### D3: `ConfigPaths` as a nested module in `FileWriter.fs`

**Decision**: Define `module ConfigPaths` inside the `FsAgent.Writers` namespace in `FileWriter.fs`, with two public functions: `resolveGlobalRoot (harness) (copilotRoot: string option)` and `resolveProjectRoot (harness) (rootDir: string)`.

**Rationale**: Keeps all path logic in one file. The existing private `resolveGlobalRoot` and `harnessProjectRoot` become delegates or are replaced by these public functions. No new file is needed.

### D4: `Testably.Abstractions` for `IFileSystem`

**Decision**: Add `Testably.Abstractions` NuGet package to `FsAgent.fsproj` and `Testably.Abstractions.Testing` to the test project.

**Rationale**: Testably is the de-facto `System.IO.Abstractions` successor for .NET, provides `MockFileSystem` out of the box, and has no transitive conflicts with the current stack. The `IFileSystem` interface it exposes is the injection point for `AgentFileWriter`.

**Alternative considered**: Custom `IFileSystem` interface — rejected; reinventing the abstraction adds maintenance burden with no benefit.

### D5: `AgentFileWriter` as a type in `FsAgent.Writers` namespace

**Decision**: Implement as an F# class (not a record or module) to allow optional constructor parameters (`?configure`, `?copilotRoot`, `?folderVariant`) with clean OO semantics. Members `WriteAgent`, `WriteSkill`, `WriteCommand` mirror module-level functions.

**Rationale**: An injectable class is the natural fit here — it holds injected dependencies (`fileSystem`, `scope`) and dispatches to the same `resolveOutputPath` + render pipeline used by the module functions.

### D6: Copilot global scope resolution order

**Decision**: `ConfigPaths.resolveGlobalRoot Copilot (Some r)` → `r`; `ConfigPaths.resolveGlobalRoot Copilot None` → check `COPILOT_GLOBAL_ROOT` env var → if set, use it; else raise `NotSupportedException`.

**Rationale**: Explicit parameter takes highest priority (test/prod injection), env var is the ambient config mechanism, exception is the last resort. Matches the documented priority order in `docs/file-writer.md`.

## Risks / Trade-offs

- **Breaking default path for OpenCode skills** → Mitigation: document in CHANGELOG; existing tests `A 7.3`/`A 7.4` updated to assert new path; the arity-4 overload means no compile error for existing callers, but output location changes.
- **`Testably.Abstractions` version pinning** → Mitigation: pin to a stable minor version; verify no conflict with `YamlDotNet` or `System.Text.Json` transitive graph before merging.
- **`COPILOT_GLOBAL_ROOT` leaking into unrelated test runs** → Mitigation: tests that assert the raise case must unset the env var; tests using the env var path should set/restore it within the test.
