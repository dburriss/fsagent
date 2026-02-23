## 1. Dependencies

- [x] 1.1 Add `Testably.Abstractions` NuGet package to `src/FsAgent/FsAgent.fsproj`
- [x] 1.2 Add `Testably.Abstractions.Testing` NuGet package to `tests/FsAgent.Tests/FsAgent.Tests.fsproj`
- [x] 1.3 Verify build succeeds with new packages (`dotnet build`)

## 2. FolderVariant Option

- [x] 2.1 Add `FolderVariant` DU to `FileWriter.fs`: `AgentsFolder | OpencodeFolder | ClaudeFolder`
- [x] 2.2 Add arity-5 overload of `resolveOutputPath` accepting `folderVariant: FolderVariant` — routes OpenCode + Project + SkillArtifact to `.agents/skills/`, `.opencode/skills/`, or `.claude/skills/`; `ClaudeFolder` also affects Copilot + Project
- [x] 2.3 Update arity-4 overload of `resolveOutputPath` to delegate to arity-5 with `AgentsFolder` default
- [x] 2.4 Update `writeFile` and `writeSkill` module functions to pass `AgentsFolder` default
- [x] 2.5 Update test `A 7.3` to assert `.agents/skills/` for OpenCode project-scope (was `.opencode/skills/`)
- [x] 2.6 Update test `A 7.4` to assert correct OpenCode global skill path suffix
- [x] 2.7 Add test `A 7.9`: arity-4 default gives `.agents/skills/` for OpenCode
- [x] 2.8 Add test `A 7.10`: arity-5 with `OpencodeFolder` gives `.opencode/skills/` for OpenCode
- [x] 2.9 Update test `A 7.11`: `AgentsFolder`/`OpencodeFolder` have no effect on ClaudeCode path
- [x] 2.10 Add test `A 7.19`: `ClaudeFolder` gives `.claude/skills/` for OpenCode
- [x] 2.11 Add test `A 7.20`: `ClaudeFolder` gives `.claude/skills/` for Copilot
- [x] 2.12 Add test `A 7.21`: `AgentsFolder`/`OpencodeFolder` have no effect on Copilot path

## 3. ConfigPaths Module

- [x] 3.1 Extract private `resolveGlobalRoot` and `harnessProjectRoot` logic into a new public `module ConfigPaths` in `FileWriter.fs`
- [x] 3.2 Implement `ConfigPaths.resolveProjectRoot (harness: AgentHarness) (rootDir: string) : string`
- [x] 3.3 Implement `ConfigPaths.resolveGlobalRoot (harness: AgentHarness) (copilotRoot: string option) : string` with priority: explicit param → `COPILOT_GLOBAL_ROOT` env var → `NotSupportedException` (for Copilot); direct resolution for Opencode and ClaudeCode
- [x] 3.4 Update internal `scopeRoot` to delegate to `ConfigPaths` functions
- [x] 3.5 Add `ConfigPaths` re-export to `Library.fs` under `open FsAgent`
- [x] 3.6 Add tests `A 7.x` for `ConfigPaths.resolveProjectRoot`: all three harnesses
- [x] 3.7 Add tests `A 7.x` for `ConfigPaths.resolveGlobalRoot`: Opencode, ClaudeCode, Copilot with explicit root, Copilot with env var, Copilot raise

## 4. Copilot Global Scope via Env Var

- [x] 4.1 Implement `COPILOT_GLOBAL_ROOT` env-var fallback in `ConfigPaths.resolveGlobalRoot` for Copilot (explicit param → env var → raise)
- [x] 4.2 Update test `A 7.2` to confirm raise only when env var is also unset
- [x] 4.3 Add test `A 7.x`: Copilot global with `COPILOT_GLOBAL_ROOT` set returns env var value
- [x] 4.4 Add test `A 7.x`: explicit `copilotRoot` overrides env var

## 5. AgentFileWriter Class

- [x] 5.1 Implement `AgentFileWriter` type in `FileWriter.fs` with constructor `(fileSystem: IFileSystem, scope: WriteScope, ?configure, ?copilotRoot, ?folderVariant)`
- [x] 5.2 Implement `WriteAgent(agent: Agent, harness: AgentHarness) -> string` — render via `AgentWriter.renderAgent`, write via `fileSystem.File.WriteAllText`, create dirs via `fileSystem.Directory.CreateDirectory`
- [x] 5.3 Implement `WriteSkill(skill: Skill, harness: AgentHarness) -> string` — respects `?folderVariant` (defaults to `AgentsFolder`)
- [x] 5.4 Implement `WriteCommand(cmd: SlashCommand, harness: AgentHarness, ?ns: string) -> string`
- [x] 5.5 Add `AgentFileWriter` re-export to `Library.fs` under `open FsAgent`
- [x] 5.6 Add test `C 9.1`: `AgentFileWriter` with `MockFileSystem` — `WriteAgent` creates file at correct path, no real I/O
- [x] 5.7 Add test `C 9.2`: `AgentFileWriter` — `WriteSkill` for OpenCode defaults to `.agents/skills/`
- [x] 5.8 Add test `C 9.3`: `AgentFileWriter` — `WriteSkill` for ClaudeCode uses `.claude/skills/`
- [x] 5.9 Add test `C 9.4`: `AgentFileWriter` — `WriteCommand` for Copilot uses `.prompt.md`
- [x] 5.10 Add test `C 9.5`: `AgentFileWriter` — `WriteCommand` for ClaudeCode with namespace uses subdirectory
- [x] 5.11 Add test `C 9.6`: `AgentFileWriter` constructed with `copilotRoot` uses it for Copilot global scope
- [x] 5.12 Add test `C 9.7`: `AgentFileWriter` with `ClaudeFolder` — `WriteSkill` for OpenCode uses `.claude/skills/`
- [x] 5.13 Add test `C 9.8`: `AgentFileWriter` with `ClaudeFolder` — `WriteSkill` for Copilot uses `.claude/skills/`

## 6. Verification

- [x] 6.1 Run `dotnet build` — zero errors and warnings
- [x] 6.2 Run `dotnet test` — all tests pass including updated and new tests
- [x] 6.3 Update `CHANGELOG.md` — document breaking default path change for OpenCode skills and new public surfaces
