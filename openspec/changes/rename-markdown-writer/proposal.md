## Why

`MarkdownWriter` is misleading — the module already produces Markdown, JSON, and YAML output. The `write*` prefix on public functions implies file I/O rather than string transformation. `writeMarkdown` is a redundant alias for `writeAgent`. Renaming before 1.0 avoids carrying misleading names into the stable API.

## What Changes

- **BREAKING** Rename `module MarkdownWriter` → `module AgentWriter` in `src/FsAgent/Writers.fs`
- **BREAKING** Rename `writeAgent` → `renderAgent` (public)
- **BREAKING** Rename `writePrompt` → `renderPrompt` (public)
- **BREAKING** Rename `writeCommand` → `renderCommand` (public)
- **BREAKING** Remove `writeMarkdown` (redundant alias for `writeAgent`)
- Rename `writeMd` → `renderMd` (private, no public API impact)
- Rename `writeJson` → `renderJson` (private, no public API impact)
- Rename `writeYaml` → `renderYaml` (private, no public API impact)
- Update `Library.fs` re-export alias from `MarkdownWriter` → `AgentWriter`
- Rename test file `MarkdownWriterTests.fs` → `AgentWriterTests.fs` and update `.fsproj`
- Update all references in tests, examples, scripts, and documentation

## Capabilities

### New Capabilities

_(none — this is a pure rename refactor with no new functionality)_

### Modified Capabilities

- `markdown-writer`: Requirements reference `MarkdownWriter`, `writeAgent`, `writePrompt`, `writeCommand` by name — update to `AgentWriter`, `renderAgent`, `renderPrompt`, `renderCommand`

## Impact

- **`src/FsAgent/Writers.fs`**: Module rename + 7 function renames; remove `writeMarkdown`
- **`src/FsAgent/Library.fs`**: Re-export alias update
- **`tests/FsAgent.Tests/`**: `MarkdownWriterTests.fs` → `AgentWriterTests.fs`; update all `write*` call sites in `CommandTests.fs`, `PromptTests.fs`, `AgentPromptIntegrationTests.fs`
- **`tests/FsAgent.Tests/FsAgent.Tests.fsproj`**: Update `<Compile Include>`
- **`examples/`**: `agents-md.fsx`, `toon.fsx`, `typed-tools.fsx`
- **`SmokeTest.fsx`**: Update call sites
- **Docs**: `ARCHITECTURE.md`, `README.md`, `CHANGELOG.md`, `MIGRATION.md`, `CLAUDE.md`, `docs/using-ast.md`, `examples/README.md`, `knowledge/fsagent-api.md`
- **`openspec/specs/markdown-writer/spec.md`**: Update symbol names in requirements
- **`plans/`** and **`openspec/changes/`**: Textual references (no compile impact)
