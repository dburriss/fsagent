# Changelog

## [Unreleased]

### Added
- Initial F# solution setup with FsAgent library and FsAgent.Tests projects
- Added YamlDotNet and System.Text.Json dependencies
- Basic build and test instructions in README.md
- OpenSpec: agent-ast spec adds constructors for `instructions`, `context`, `result`, `example`, `examples`
- Markdown writer with configurable options for output format, type, import inclusion, heading renames, and custom writers
- Agent DSL: Top-level F# computation expression `agent { ... }` for ergonomic agent authoring
- Frontmatter helpers: `fmStr`, `fmNum`, `fmBool`, `fmList`, `fmMap` for generic metadata
- Import inference: `importRef` infers format from file extensions (.yml/.yaml/.json/.toon)
