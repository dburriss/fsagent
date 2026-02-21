## 1. Project Structure

- [x] 1.1 Add `FileWriter.fs` to `src/FsAgent/` after `Writers.fs` in `FsAgent.fsproj` compile order
- [x] 1.2 Add `FileWriterTests.fs` to `tests/FsAgent.Tests/` test project

## 2. Core Types

- [x] 2.1 Define `WriteScope` DU (`Project of rootDir: string | Global`) in `FileWriter.fs` under `FsAgent.Writers` namespace
- [x] 2.2 Define `ArtifactKind` DU (`AgentArtifact | CommandArtifact of namespace_: string option | SkillArtifact`) in `FileWriter.fs`

## 3. Pure Path Resolution

- [x] 3.1 Implement `resolveGlobalRoot` — returns the OS-correct base path per harness (`~/.config/opencode/` or `~/.claude/`) using `Environment.GetFolderPath` and `RuntimeInformation.IsOSPlatform`; raises `NotSupportedException` for `Copilot`
- [x] 3.2 Implement `resolveOutputPath : AgentHarness -> ArtifactKind -> string -> WriteScope -> string` — pure function; uses `Path.Combine` throughout, calls `Path.GetFullPath` on project root; encodes all harness × kind × scope path conventions from the spec
- [x] 3.3 Handle Copilot `.prompt.md` double-extension in `resolveOutputPath` for `CommandArtifact`
- [x] 3.4 Handle ClaudeCode namespaced commands (optional subdirectory) in `resolveOutputPath`
- [x] 3.5 Handle skill `<name>/SKILL.md` layout in `resolveOutputPath` for all harnesses

## 4. I/O Layer

- [x] 4.1 Implement `writeFile : AgentHarness -> ArtifactKind -> string -> WriteScope -> string -> string` — calls `resolveOutputPath`, creates intermediate directories with `Directory.CreateDirectory`, writes content with `File.WriteAllText`, returns resolved path

## 5. Convenience Write Functions

- [x] 5.1 Implement `writeAgent : Agent -> AgentHarness -> WriteScope -> (Options -> unit) -> string` — extracts name from `agent.Frontmatter["name"]`, raises `ArgumentException` if missing, calls `renderAgent` then `writeFile`
- [x] 5.2 Implement `writeSkill : Skill -> AgentHarness -> WriteScope -> (Options -> unit) -> string` — extracts name from `skill.Frontmatter["name"]`, raises `ArgumentException` if missing, calls `renderSkill` then `writeFile`
- [x] 5.3 Implement `writeCommand : SlashCommand -> AgentHarness -> WriteScope -> namespace_: string option -> (Options -> unit) -> string` — uses `cmd.Name`, calls `renderCommand` then `writeFile`

## 6. Re-exports

- [x] 6.1 Re-export `WriteScope`, `ArtifactKind`, `resolveOutputPath`, `writeFile`, `writeAgent`, `writeSkill`, `writeCommand` from `Library.fs` DSL module

## 7. Tests — Path Resolution (pure, no I/O)

- [x] 7.1 Test `resolveOutputPath` for all three harnesses with `AgentArtifact` and `Project` scope (3 assertions)
- [x] 7.2 Test `resolveOutputPath` raises `NotSupportedException` for `Copilot` + `Global`
- [x] 7.3 Test `resolveOutputPath` for all three harnesses with `SkillArtifact` and `Project` scope — verify `<name>/SKILL.md` layout
- [x] 7.4 Test `resolveOutputPath` with `Global` scope for `Opencode` and `ClaudeCode` skill paths (path ends with expected suffix)
- [x] 7.5 Test `resolveOutputPath` for Opencode and ClaudeCode commands with `CommandArtifact None` and `Project` scope
- [x] 7.6 Test `resolveOutputPath` for Copilot command returns path ending in `.prompt.md`
- [x] 7.7 Test `resolveOutputPath` for ClaudeCode namespaced command (`CommandArtifact (Some "opsx")`)
- [x] 7.8 Test `resolveOutputPath` with `Project "."` produces an absolute path (no leading `.`)

## 8. Tests — File I/O (filesystem, temp directory)

- [x] 8.1 Test `writeFile` creates file at resolved path in a temp directory
- [x] 8.2 Test `writeFile` creates intermediate directories that do not exist
- [x] 8.3 Test `writeFile` overwrites an existing file with new content
- [x] 8.4 Test `writeFile` returns the resolved path string
- [x] 8.5 Test `writeAgent` raises `ArgumentException` when `Agent` has no `name` in frontmatter
- [x] 8.6 Test `writeAgent` writes to correct path and file content matches rendered output
- [x] 8.7 Test `writeSkill` raises `ArgumentException` when `Skill` has no `name` in frontmatter
- [x] 8.8 Test `writeSkill` writes to correct `<name>/SKILL.md` subdirectory layout
- [x] 8.9 Test `writeCommand` for Copilot writes file with `.prompt.md` extension
- [x] 8.10 Test `writeCommand` for ClaudeCode with namespace writes to correct subdirectory

## 9. CHANGELOG & Docs

- [x] 9.1 Add entry to `CHANGELOG.md` under `[Unreleased]` describing the new `FileWriter` module and public API
