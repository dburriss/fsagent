## 1. Source: Writers.fs

- [ ] 1.1 Rename `module MarkdownWriter` to `module AgentWriter` in `src/FsAgent/Writers.fs`
- [ ] 1.2 Rename `writeAgent` → `renderAgent` (public)
- [ ] 1.3 Rename `writePrompt` → `renderPrompt` (public)
- [ ] 1.4 Rename `writeCommand` → `renderCommand` (public)
- [ ] 1.5 Remove `writeMarkdown` alias entirely
- [ ] 1.6 Rename `writeMd` → `renderMd` (private)
- [ ] 1.7 Rename `writeJson` → `renderJson` (private)
- [ ] 1.8 Rename `writeYaml` → `renderYaml` (private)

## 2. Source: Library.fs

- [ ] 2.1 Update re-export alias in `src/FsAgent/Library.fs` from `MarkdownWriter` to `AgentWriter`

## 3. Tests: Project File and File Rename

- [ ] 3.1 Rename file `tests/FsAgent.Tests/MarkdownWriterTests.fs` → `AgentWriterTests.fs` on disk
- [ ] 3.2 Update `<Compile Include="MarkdownWriterTests.fs" />` → `<Compile Include="AgentWriterTests.fs" />` in `tests/FsAgent.Tests/FsAgent.Tests.fsproj`

## 4. Tests: Call Sites

- [ ] 4.1 Replace all `MarkdownWriter.` → `AgentWriter.` and `write*` → `render*` in `tests/FsAgent.Tests/AgentWriterTests.fs`
- [ ] 4.2 Replace all `MarkdownWriter.` → `AgentWriter.` and `write*` → `render*` in `tests/FsAgent.Tests/CommandTests.fs`
- [ ] 4.3 Replace all `MarkdownWriter.` → `AgentWriter.` and `write*` → `render*` in `tests/FsAgent.Tests/PromptTests.fs`
- [ ] 4.4 Replace all `MarkdownWriter.` → `AgentWriter.` and `write*` → `render*` in `tests/FsAgent.Tests/AgentPromptIntegrationTests.fs`

## 5. Build and Test Verification

- [ ] 5.1 Run `dotnet build` — must produce zero errors
- [ ] 5.2 Run `dotnet test` — all tests must pass

## 6. Examples and Scripts

- [ ] 6.1 Update `examples/agents-md.fsx` — replace `MarkdownWriter.*` and `write*` references
- [ ] 6.2 Update `examples/toon.fsx` — replace `MarkdownWriter.*` and `write*` references
- [ ] 6.3 Update `examples/typed-tools.fsx` — replace `MarkdownWriter.*` and `write*` references
- [ ] 6.4 Update `SmokeTest.fsx` — replace `MarkdownWriter.*` and `write*` references

## 7. Primary Documentation

- [ ] 7.1 Update `ARCHITECTURE.md` — rename module in diagram; replace `writeAgent`/`writePrompt` with `renderAgent`/`renderPrompt`; move unimplemented `IWriter`/composable writer sections to a "Future: Composable Writer Hierarchy" section at bottom
- [ ] 7.2 Update `README.md` — replace all `MarkdownWriter.*` and `write*` references
- [ ] 7.3 Update `CHANGELOG.md` — add entry for rename; update historical references
- [ ] 7.4 Update `MIGRATION.md` — add migration note for all renamed symbols and removed `writeMarkdown`

## 8. Spec Update

- [ ] 8.1 Apply delta to `openspec/specs/markdown-writer/spec.md` — update requirement names and scenario text for `renderPrompt`, `renderAgent`, `renderCommand`, `AgentWriter`

## 9. Internal Docs Sweep

- [ ] 9.1 Update `CLAUDE.md` — replace old symbol references
- [ ] 9.2 Update `docs/using-ast.md` — replace old symbol references
- [ ] 9.3 Update `examples/README.md` — replace old symbol references
- [ ] 9.4 Update `knowledge/fsagent-api.md` — replace old symbol references
- [ ] 9.5 Update all `plans/*.md` files — replace old symbol references
- [ ] 9.6 Update `openspec/changes/add-slash-command-dsl/**/*.md` — replace old symbol references
- [ ] 9.7 Update `openspec/changes/archive/**/*.md` — replace old symbol references (low priority, historical)
