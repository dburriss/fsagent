# Changelog

## [Unreleased]

## [0.3.0] - 2026-02-02

### Added
- **Tool discriminated union**: Type-safe tool configuration with `Write`, `Edit`, `Bash`, `WebFetch`, `Todo`, and `Custom of string` cases
- **ClaudeCode harness**: New `AgentHarness` case for Claude Code platform with capitalized tool names
- **Harness-specific tool name mapping**: Tools automatically map to correct names based on target platform (e.g., `Write` → "write" for Opencode, "Write" for ClaudeCode)
- **Typed tools operation**: Agent builder `tools` operation now accepts `Tool list` for compile-time safety and IDE autocomplete
- **Typed disallowedTools operation**: Agent builder `disallowedTools` operation now accepts `Tool list` instead of `string list`
- **FsAgent.Tools namespace**: New dedicated namespace for Tool type and related tool functionality

### Changed
- **BREAKING**: `Tool` type moved from `FsAgent.AST` to `FsAgent.Tools` namespace - users must update imports from `open FsAgent.AST` to `open FsAgent.Tools`
- **BREAKING**: `AgentFormat` type renamed to `AgentHarness` throughout codebase to better represent execution platform
- **BREAKING**: `tools` operation signature changed from `obj list` to `Tool list`
- **BREAKING**: `disallowedTools` operation signature changed from `string list` to `Tool list`
- **BREAKING**: Frontmatter storage changed - tools stored as `Tool list` objects, not string maps
- Tool name resolution moved from agent definition time to write time, enabling harness-agnostic agent definitions
- Tools are now always output as a list format, with disabled tools omitted from the output

### Removed
- **BREAKING**: `toolMap` operation removed from agent builder - use `tools` + `disallowedTools` instead
- **BREAKING**: `ToolFormat` option removed from `MarkdownWriter.Options` - tools are now always output as a list (equivalent to the old `ToolsList` format). The `ToolFormat` option (`ToolsList`, `ToolsMap`, `Auto`) has been removed to simplify the API. Since `Auto` defaulted to `ToolsList` for all harnesses, the format option was redundant. Disabled tools are simply omitted from the output list.

### Migration
See [MIGRATION.md](MIGRATION.md) for complete migration guide from v1.x to v2.0.

**Quick migration examples:**
```fsharp
// Before (v1.x)
open FsAgent.AST
tools ["write" :> obj; "bash" :> obj]
disallowedTools ["bash"]
opts.OutputFormat <- MarkdownWriter.AgentFormat.Opencode

// After (v2.0)
open FsAgent.Tools  // Tool type now in Tools namespace
tools [Write; Bash]
disallowedTools [Bash]
opts.OutputFormat <- Opencode
```

---

## [0.2.0] - Previous Release

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
