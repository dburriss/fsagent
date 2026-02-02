## Why

Current tool configuration is stringly-typed with no compile-time safety or IDE discoverability. The `AgentFormat` naming is unclear (suggests file format rather than execution platform), and the library doesn't support Claude Code as a target platform. Multiple redundant operations (`tools`, `toolMap`, `disallowedTools`) create API confusion. Different agent harnesses use different tool names, but the library converts tools to strings too early, preventing platform-specific mappings.

## What Changes

- **BREAKING**: Rename `AgentFormat` → `AgentHarness` (better semantics for execution platform)
- **BREAKING**: Add `ClaudeCode` case to `AgentHarness` (third platform alongside Opencode and Copilot)
- **BREAKING**: Add `Tool` discriminated union for type-safe tool references (Write, Edit, Bash, WebFetch, Todo, Custom)
- **BREAKING**: Update `tools` operation signature from `obj list` to `Tool list`
- **BREAKING**: Update `disallowedTools` operation signature from `string list` to `Tool list`
- **BREAKING**: Remove redundant `toolMap` operation
- **BREAKING**: Change frontmatter storage from string maps to Tool lists (delayed string conversion)
- Add harness-specific tool name mapping in Writer (`toolToString` function)
- Update `formatToolsFrontmatter` to extract Tool lists and convert based on target harness

## Capabilities

### New Capabilities

- `agent-harness`: Rename AgentFormat to AgentHarness and add ClaudeCode platform support. Defines the three execution platforms (Opencode, Copilot, ClaudeCode) with clear semantics.

- `typed-tools`: Type-safe tool configuration using discriminated unions. Provides compile-time checking, IDE discoverability, and harness-specific tool name mapping at write time.

### Modified Capabilities

<!-- No existing capabilities are being modified - this is all new functionality -->

## Impact

**Files Modified**:
- `src/FsAgent/AST.fs` - Add Tool discriminated union
- `src/FsAgent/Library.fs` - Re-export Tool type
- `src/FsAgent/Agent.fs` - Update tools/disallowedTools operations, remove toolMap
- `src/FsAgent/Writers.fs` - Rename AgentFormat → AgentHarness, add ClaudeCode, add toolToString mapping, update formatToolsFrontmatter
- `tests/FsAgent.Tests/MarkdownWriterTests.fs` - Update ~14 tests, remove 1 test

**Breaking Changes**:
- All external code using `AgentFormat` must update to `AgentHarness`
- Agent builder tool operations have different signatures
- Tool storage format in frontmatter changes from strings to Tool types
- `toolMap` operation removed (migrate to `tools` + `disallowedTools`)

**Version**: Requires major version bump (e.g., 1.0.0 → 2.0.0)

**Benefits**:
- Type safety with compile-time checking
- IDE autocomplete for universal tools
- Platform-specific tool name mapping
- Clearer API semantics
- Reduced API surface (removed toolMap)
