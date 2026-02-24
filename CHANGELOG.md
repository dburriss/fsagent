# Changelog

## [Unreleased]

### Added
- `FolderVariant` DU with `ClaudeFolder` case — routes OpenCode and Copilot skills to `.claude/skills/`
- `Markdown` data format variant — `.md`/`.markdown` imports rendered as raw text
- `sectionFrom` DSL operation — loads named section content from an external file
- `SectionStyle` option (`Markdown` | `Xml`) on `AgentWriter.Options`
- `ValidationException` on `AgentWriter` — typed exception for render validation errors
- `FsAgent.Toon` project — pure F# TOON v1.2 parser/serializer (`netstandard2.0`, no dependencies)
- `ToonSerializer` hook on `AgentWriter.Options` for normalizing TOON imports
- `FileWriter` module — harness-aware path resolution and file writing (`WriteScope`, `ArtifactKind`)
- `AgentFileWriter` class — injectable writer using `IFileSystem` for testable I/O
- `ConfigPaths` module — pure functions for resolving project/global roots per harness
- `COPILOT_GLOBAL_ROOT` env-var fallback for Copilot global scope
- `skill { ... }` CE builder with YAML frontmatter and `renderSkill` writer
- `command { ... }` CE builder for slash commands and `renderCommand` writer
- `{{{tool <Name>}}}` template syntax — resolves to harness-correct tool names at write time

### Changed
- **BREAKING**: `OpenCodeSkillPath` renamed to `FolderVariant`; `?skillPath` → `?folderVariant`
- **BREAKING**: OpenCode project-scope skill default path changed to `.agents/skills/` (use `OpencodeFolder` to restore)
- **BREAKING**: `CommandBuilder` semantic ops removed — use `prompt { ... }` composition
- **BREAKING**: `module MarkdownWriter` → `module AgentWriter`; `write*` → `render*`; `writeMarkdown` removed
- `renderCommand` and `renderSkill` validate required fields, reporting all violations in a single `ValidationException`
- `renderAgent` Copilot validation uses `ValidationException` with per-field reporting

See [MIGRATION.md](MIGRATION.md) for migration guide.

## [0.3.1] - 2026-02-04

### Changed
- Opencode tools output as struct/map with boolean values; disabled tools shown as `false`
- Tools sorted alphabetically for deterministic output

### Fixed
- Malformed YAML when Copilot/ClaudeCode agents have only `disallowedTools` with no enabled tools

## [0.3.0] - 2026-02-02

### Added
- `Tool` DU — type-safe tool configuration (`Write`, `Edit`, `Bash`, `WebFetch`, `Todo`, `Custom of string`)
- `ClaudeCode` harness with platform-specific tool name mapping
- `FsAgent.Tools` namespace

### Changed
- **BREAKING**: `Tool` type moved from `FsAgent.AST` to `FsAgent.Tools`
- **BREAKING**: `AgentFormat` renamed to `AgentHarness`
- **BREAKING**: `tools` accepts `Tool list` (was `obj list`); `disallowedTools` accepts `Tool list` (was `string list`)
- **BREAKING**: Frontmatter stores tools as `Tool list`, not string maps
- Tool name resolution deferred to write time — agent definitions are now harness-agnostic

### Removed
- **BREAKING**: `toolMap` operation — use `tools` + `disallowedTools`
- **BREAKING**: `ToolFormat` option — tools always output as list; disabled tools omitted

See [MIGRATION.md](MIGRATION.md) for migration guide.

## [0.2.0] - Previous Release

### Added
- `Prompt` type with `prompt { ... }` CE builder for reusable prompts
- `Template` and `TemplateFile` nodes with Fue-based `{{{variable}}}` substitution
- `template` / `templateFile` operations on prompt builder
- `renderPrompt` and `renderAgent` writer functions
- `importRaw` DSL operation for raw content embedding without code fences
- `DisableCodeBlockWrapping` writer option
- Prompt metadata operations (`name`, `description`, `author`, `version`, `license`)
- Agent metadata operations (`model`, `temperature`, `maxTokens`, `tools`)

### Changed
- **BREAKING**: Agent builder semantic ops (`role`, `objective`, etc.) removed — use `prompt { ... }` composition
- **BREAKING**: Codebase split into `AST.fs`, `Prompt.fs`, `Agent.fs`, `Writers.fs` with new namespaces
- **BREAKING**: `import` now wraps content in fenced code blocks; use `importRaw` for raw embedding
- **BREAKING**: `ImportInclusion` type removed — code block wrapping controlled by DSL operation choice
- Target changed to `netstandard2.0`

See [MIGRATION.md](MIGRATION.md) for migration guide.

## [0.1.0] - 2025-12-15

### Added
- `agent { ... }` CE builder for agent authoring
- Markdown writer with configurable options (format, heading renames, custom writers)
- Section constructors: `instructions`, `context`, `result`, `example`, `examples`
- Frontmatter helpers: `fmStr`, `fmNum`, `fmBool`, `fmList`, `fmMap`
- `importRef` with format inference from file extensions
