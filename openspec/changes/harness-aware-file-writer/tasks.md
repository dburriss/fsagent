## 1. Project Structure

- [ ] 1.1 Add `FileWriter.fs` to `src/FsAgent/` after `Writers.fs` in `FsAgent.fsproj` compile order
- [ ] 1.2 Add `FileWriterTests.fs` to `tests/FsAgent.Tests/` test project

## 2. Core Types

- [ ] 2.1 Define `WriteScope` DU (`Project of rootDir: string | Global`) in `FileWriter.fs` under `FsAgent.Writers` namespace
- [ ] 2.2 Define `ArtifactKind` DU (`AgentArtifact | CommandArtifact of namespace_: string option | SkillArtifact`) in `FileWriter.fs`

## 3. Pure Path Resolution

- [ ] 3.1 Implement `resolveGlobalRoot` — returns the OS-correct base path per harness (`~/.config/opencode/` or `~/.claude/`) using `Environment.GetFolderPath` and `RuntimeInformation.IsOSPlatform`; raises `NotSupportedException` for `Copilot`
- [ ] 3.2 Implement `resolveOutputPath : AgentHarness -> ArtifactKind -> string -> WriteScope -> string` — pure function; uses `Path.Combine` throughout, calls `Path.GetFullPath` on project root; encodes all harness × kind × scope path conventions from the spec
- [ ] 3.3 Handle Copilot `.prompt.md` double-extension in `resolveOutputPath` for `CommandArtifact`
- [ ] 3.4 Handle ClaudeCode namespaced commands (optional subdirectory) in `resolveOutputPath`
- [ ] 3.5 Handle skill `<name>/SKILL.md` layout in `resolveOutputPath` for all harnesses

## 4. I/O Layer

- [ ] 4.1 Implement `writeFile : AgentHarness -> ArtifactKind -> string -> WriteScope -> string -> string` — calls `resolveOutputPath`, creates intermediate directories with `Directory.CreateDirectory`, writes content with `File.WriteAllText`, returns resolved path

## 5. Convenience Write Functions

- [ ] 5.1 Implement `writeAgent : Agent -> AgentHarness -> WriteScope -> (Options -> unit) -> string` — extracts name from `agent.Frontmatter["name"]`, raises `ArgumentException` if missing, calls `renderAgent` then `writeFile`
- [ ] 5.2 Implement `writeSkill : Skill -> AgentHarness -> WriteScope -> (Options -> unit) -> string` — extracts name from `skill.Frontmatter["name"]`, raises `ArgumentException` if missing, calls `renderSkill` then `writeFile`
- [ ] 5.3 Implement `writeCommand : SlashCommand -> AgentHarness -> WriteScope -> namespace_: string option -> (Options -> unit) -> string` — uses `cmd.Name`, calls `renderCommand` then `writeFile`

## 6. Re-exports

- [ ] 6.1 Re-export `WriteScope`, `ArtifactKind`, `resolveOutputPath`, `writeFile`, `writeAgent`, `writeSkill`, `writeCommand` from `Library.fs` DSL module

## 7. Tests — Path Resolution (pure, no I/O)

- [ ] 7.1 Test `resolveOutputPath` for all three harnesses with `AgentArtifact` and `Project` scope (3 assertions)
- [ ] 7.2 Test `resolveOutputPath` raises `NotSupportedException` for `Copilot` + `Global`
- [ ] 7.3 Test `resolveOutputPath` for all three harnesses with `SkillArtifact` and `Project` scope — verify `<name>/SKILL.md` layout
- [ ] 7.4 Test `resolveOutputPath` with `Global` scope for `Opencode` and `ClaudeCode` skill paths (path ends with expected suffix)
- [ ] 7.5 Test `resolveOutputPath` for Opencode and ClaudeCode commands with `CommandArtifact None` and `Project` scope
- [ ] 7.6 Test `resolveOutputPath` for Copilot command returns path ending in `.prompt.md`
- [ ] 7.7 Test `resolveOutputPath` for ClaudeCode namespaced command (`CommandArtifact (Some "opsx")`)
- [ ] 7.8 Test `resolveOutputPath` with `Project "."` produces an absolute path (no leading `.`)

## 8. Tests — File I/O (filesystem, temp directory)

- [ ] 8.1 Test `writeFile` creates file at resolved path in a temp directory
- [ ] 8.2 Test `writeFile` creates intermediate directories that do not exist
- [ ] 8.3 Test `writeFile` overwrites an existing file with new content
- [ ] 8.4 Test `writeFile` returns the resolved path string
- [ ] 8.5 Test `writeAgent` raises `ArgumentException` when `Agent` has no `name` in frontmatter
- [ ] 8.6 Test `writeAgent` writes to correct path and file content matches rendered output
- [ ] 8.7 Test `writeSkill` raises `ArgumentException` when `Skill` has no `name` in frontmatter
- [ ] 8.8 Test `writeSkill` writes to correct `<name>/SKILL.md` subdirectory layout
- [ ] 8.9 Test `writeCommand` for Copilot writes file with `.prompt.md` extension
- [ ] 8.10 Test `writeCommand` for ClaudeCode with namespace writes to correct subdirectory

## 9. CHANGELOG & Docs

- [ ] 9.1 Add entry to `CHANGELOG.md` under `[Unreleased]` describing the new `FileWriter` module and public API
