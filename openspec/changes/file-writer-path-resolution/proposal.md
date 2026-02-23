## Why

`FileWriter` has four gaps that limit its correctness and testability: OpenCode project-scope skills are written to `.opencode/skills/` instead of the cross-tool spec path `.agents/skills/`; Copilot global scope always raises rather than checking `COPILOT_GLOBAL_ROOT`; path-resolution helpers are private with no public surface for consumers who only need paths; and there is no injectable `AgentFileWriter` class, making the module untestable without real I/O.

## What Changes

- Add `FolderVariant` DU (`AgentsFolder` | `OpencodeFolder` | `ClaudeFolder`) to control which root project skills are written to
- **BREAKING**: Default OpenCode project-scope skill path changes from `.opencode/skills/` to `.agents/skills/` (cross-tool Agent Skills spec path)
- Add arity-4 overload of `resolveOutputPath` (backward-compatible, defaults to `AgentsFolder`); add arity-5 overload taking explicit `FolderVariant`
- `ClaudeFolder` routes both OpenCode and Copilot project-scope skills to `.claude/skills/` — the cross-tool path supported by all three harnesses
- Implement `COPILOT_GLOBAL_ROOT` env-var fallback in Copilot global scope resolution before raising `NotSupportedException`
- Expose public `ConfigPaths` module with `resolveGlobalRoot` (harness × copilotRoot option) and `resolveProjectRoot` (harness × rootDir)
- Implement `AgentFileWriter` class accepting `IFileSystem` (Testably.Abstractions) for in-memory-testable writes
- Re-export `ConfigPaths` and `AgentFileWriter` from `Library.fs` under `open FsAgent`
- Add `Testably.Abstractions` NuGet dep to `FsAgent` project; `Testably.Abstractions.Testing` to test project

## Capabilities

### New Capabilities

- `folder-variant-option`: `FolderVariant` DU and overloaded `resolveOutputPath` allowing callers to choose `.agents/skills/` (default), `.opencode/skills/`, or `.claude/skills/` for project-scope skills; `ClaudeFolder` affects both OpenCode and Copilot
- `copilot-global-scope`: Copilot global scope resolution via explicit `copilotRoot` parameter or `COPILOT_GLOBAL_ROOT` env var, falling back to `NotSupportedException`
- `config-paths-module`: Public `ConfigPaths` module exposing `resolveGlobalRoot` and `resolveProjectRoot` as pure path helpers
- `agent-file-writer`: Injectable `AgentFileWriter` class with `IFileSystem` abstraction, mirroring the module-level `writeAgent` / `writeSkill` / `writeCommand` functions

### Modified Capabilities

- `harness-aware-file-writer`: OpenCode project-scope skill path changes from `.opencode/skills/` to `.agents/skills/` by default; `resolveOutputPath` gains an overload

## Impact

- `src/FsAgent/FileWriter.fs` — primary changes to types, path resolution, and new class
- `src/FsAgent/Library.fs` — re-export `ConfigPaths` and `AgentFileWriter`
- `tests/FsAgent.Tests/FileWriterTests.fs` — update `A 7.3` / `A 7.4` assertions; add new tests for all new capabilities
- NuGet: add `Testably.Abstractions` to `FsAgent.fsproj`; add `Testably.Abstractions.Testing` to test project
