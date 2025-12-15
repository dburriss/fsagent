# Changelog

## [Unreleased]

### Added
- Constructors for `instructions`, `context`, `result`, `example`, `examples`
- Markdown writer with configurable options for output format, type, import inclusion, heading renames, and custom writers
- Agent DSL: Top-level F# computation expression `agent { ... }` for ergonomic agent authoring
- Frontmatter helpers: `fmStr`, `fmNum`, `fmBool`, `fmList`, `fmMap` for generic metadata
- Import inference: `importRef` infers format from file extensions (.yml/.yaml/.json/.toon)
