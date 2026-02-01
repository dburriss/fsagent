# Changelog

## [Unreleased]

### Added
- **Prompt as first-class type**: New `Prompt` type with `prompt { ... }` computation expression builder for creating reusable prompts
- **Template support**: Added `Template` and `TemplateFile` node cases with Fue-based variable substitution (`{{{variable}}}` syntax)
- **Namespace organization**: Domain-focused namespaces (`FsAgent.AST`, `FsAgent.Prompts`, `FsAgent.Agents`, `FsAgent.Writers`)
- **New writer functions**: `writePrompt` for rendering prompts without frontmatter blocks, `writeAgent` as primary agent writer
- **Template operations**: `template` and `templateFile` operations in prompt builder for dynamic content generation
- **Prompt metadata**: Prompt builder supports `name`, `description`, `author`, `version`, `license` metadata operations
- **Agent metadata operations**: Agent builder now supports `model`, `temperature`, `maxTokens`, `tools` operations
- **Fue dependency**: Integrated Fue 2.2.0 template library for Mustache-like templating
- `importRaw` DSL operation for raw content embedding without code fences
- `DisableCodeBlockWrapping` writer option to force raw output for all imports

### Changed
- **BREAKING**: Agent builder no longer supports `role`, `objective`, `instructions`, `context`, `output`, `examples` operations directly - must use prompts instead
- **BREAKING**: Code organization split into multiple files (AST.fs, Prompt.fs, Agent.fs, Writers.fs) with new namespace structure
- **BREAKING**: `import` now automatically wraps content in fenced code blocks (` ```json `, ` ```yaml `, ` ```toon `). Use `importRaw` for raw embedding
- **BREAKING**: `ImportInclusion` type removed entirely. Imports are always resolved—code block behavior is controlled by the DSL operation (`import` vs `importRaw`), not writer options
- Prompt constructors (`role`, `objective`, `instructions`, `context`, `output`, `example`, `examples`) moved from AST module to Prompt module
- `writeMarkdown` is now an alias to `writeAgent` for backward compatibility
- Library.fs now serves as backward compatibility layer with re-exports
- Target netstandard2.0 for broader compatibility

### Migration Guide
**Before (v0.1.0):**
```fsharp
open FsAgent.DSL

let agent = agent {
    role "You are a helpful assistant"
    objective "Answer questions"
    instructions "Be concise"
}
```

**After (v0.2.0):**
```fsharp
open FsAgent.Agents
open FsAgent.Prompts

let assistantPrompt = prompt {
    role "You are a helpful assistant"
    objective "Answer questions"
    instructions "Be concise"
}

let agent = agent {
    prompt assistantPrompt
}
```

**Backward compatibility:** Existing code using `open FsAgent.DSL` continues to work via re-exports.

## [0.1.0] - 2025-12-15

### Added
- Constructors for `instructions`, `context`, `result`, `example`, `examples`
- Markdown writer with configurable options for output format, type, import inclusion, heading renames, and custom writers
- Agent DSL: Top-level F# computation expression `agent { ... }` for ergonomic agent authoring
- Frontmatter helpers: `fmStr`, `fmNum`, `fmBool`, `fmList`, `fmMap` for generic metadata
- Import inference: `importRef` infers format from file extensions (.yml/.yaml/.json/.toon)
