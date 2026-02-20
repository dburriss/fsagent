## 1. Source: Writers.fs

- [x] 1.1 Rename `module MarkdownWriter` to `module AgentWriter` in `src/FsAgent/Writers.fs`
- [x] 1.2 Rename `writeAgent` → `renderAgent` (public)
- [x] 1.3 Rename `writePrompt` → `renderPrompt` (public)
- [x] 1.4 Rename `writeCommand` → `renderCommand` (public)
- [x] 1.5 Remove `writeMarkdown` alias entirely
- [x] 1.6 Rename `writeMd` → `renderMd` (private)
- [x] 1.7 Rename `writeJson` → `renderJson` (private)
- [x] 1.8 Rename `writeYaml` → `renderYaml` (private)

## 2. Source: Library.fs

- [x] 2.1 Update re-export alias in `src/FsAgent/Library.fs` from `MarkdownWriter` to `AgentWriter`

## 3. Tests: Project File and File Rename

- [x] 3.1 Rename file `tests/FsAgent.Tests/MarkdownWriterTests.fs` → `AgentWriterTests.fs` on disk
- [x] 3.2 Update `<Compile Include="MarkdownWriterTests.fs" />` → `<Compile Include="AgentWriterTests.fs" />` in `tests/FsAgent.Tests/FsAgent.Tests.fsproj`

## 4. Tests: Call Sites

- [x] 4.1 Replace all `MarkdownWriter.` → `AgentWriter.` and `write*` → `render*` in `tests/FsAgent.Tests/AgentWriterTests.fs`
- [x] 4.2 Replace all `MarkdownWriter.` → `AgentWriter.` and `write*` → `render*` in `tests/FsAgent.Tests/CommandTests.fs`
- [x] 4.3 Replace all `MarkdownWriter.` → `AgentWriter.` and `write*` → `render*` in `tests/FsAgent.Tests/PromptTests.fs`
- [x] 4.4 Replace all `MarkdownWriter.` → `AgentWriter.` and `write*` → `render*` in `tests/FsAgent.Tests/AgentPromptIntegrationTests.fs`

## 5. Build and Test Verification

- [x] 5.1 Run `dotnet build` — must produce zero errors
- [x] 5.2 Run `dotnet test` — all tests must pass

## 6. Examples and Scripts

- [x] 6.1 Update `examples/agents-md.fsx` — replace `MarkdownWriter.*` and `write*` references
- [x] 6.2 Update `examples/toon.fsx` — replace `MarkdownWriter.*` and `write*` references
- [x] 6.3 Update `examples/typed-tools.fsx` — replace `MarkdownWriter.*` and `write*` references
- [x] 6.4 Update `SmokeTest.fsx` — replace `MarkdownWriter.*` and `write*` references

## 7. Primary Documentation

- [x] 7.1 Update `ARCHITECTURE.md` — rename module in diagram; replace `writeAgent`/`writePrompt` with `renderAgent`/`renderPrompt`; move unimplemented `IWriter`/composable writer sections to a "Future: Composable Writer Hierarchy" section at bottom
- [x] 7.2 Update `README.md` — replace all `MarkdownWriter.*` and `write*` references
- [x] 7.3 Update `CHANGELOG.md` — add entry for rename; update historical references
- [x] 7.4 Update `MIGRATION.md` — add migration note for all renamed symbols and removed `writeMarkdown`

## 8. Spec Update

- [x] 8.1 Apply delta to `openspec/specs/markdown-writer/spec.md` — update requirement names and scenario text for `renderPrompt`, `renderAgent`, `renderCommand`, `AgentWriter`

## 9. Internal Docs Sweep

- [x] 9.1 Update `CLAUDE.md` — replace old symbol references
- [x] 9.2 Update `docs/using-ast.md` — replace old symbol references
- [x] 9.3 Update `examples/README.md` — replace old symbol references
- [x] 9.4 Update `knowledge/fsagent-api.md` — replace old symbol references
- [x] 9.5 Update all `plans/*.md` files — replace old symbol references
- [x] 9.6 Update `openspec/changes/add-slash-command-dsl/**/*.md` — replace old symbol references
- [x] 9.7 Update `openspec/changes/archive/**/*.md` — replace old symbol references (low priority, historical)
