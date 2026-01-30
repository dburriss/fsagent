# Changelog

## [Unreleased]

### Added
- `importRaw` DSL operation for raw content embedding without code fences
- `DisableCodeBlockWrapping` writer option to force raw output for all imports

### Changed
- **BREAKING**: `import` now automatically wraps content in fenced code blocks (` ```json `, ` ```yaml `, ` ```toon `). Use `importRaw` for raw embedding.
- **BREAKING**: `ImportInclusion` type removed entirely. Imports are always resolved—code block behavior is controlled by the DSL operation (`import` vs `importRaw`), not writer options.
- Target netstandard2.0 for broader compatibility.

## [0.1.0] - 2025-12-15

### Added
- Constructors for `instructions`, `context`, `result`, `example`, `examples`
- Markdown writer with configurable options for output format, type, import inclusion, heading renames, and custom writers
- Agent DSL: Top-level F# computation expression `agent { ... }` for ergonomic agent authoring
- Frontmatter helpers: `fmStr`, `fmNum`, `fmBool`, `fmList`, `fmMap` for generic metadata
- Import inference: `importRef` infers format from file extensions (.yml/.yaml/.json/.toon)
