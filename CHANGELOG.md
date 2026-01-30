# Changelog

## [Unreleased]

### Added
- `IncludeCodeBlock` import inclusion mode that wraps imported content in fenced code blocks with format-derived language tags (```json, ```yaml, ```toon)
- `importRaw` DSL operation for raw content embedding (preserves previous `import` behavior)

### Changed
- **BREAKING**: DSL `import` operation now signals intent for code-block wrapped embedding. Use `importRaw` for the previous raw embedding behavior.

## [0.1.0] - 2025-12-15

### Added
- Constructors for `instructions`, `context`, `result`, `example`, `examples`
- Markdown writer with configurable options for output format, type, import inclusion, heading renames, and custom writers
- Agent DSL: Top-level F# computation expression `agent { ... }` for ergonomic agent authoring
- Frontmatter helpers: `fmStr`, `fmNum`, `fmBool`, `fmList`, `fmMap` for generic metadata
- Import inference: `importRef` infers format from file extensions (.yml/.yaml/.json/.toon)
