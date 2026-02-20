## Why

Prompt text often hard-codes tool names (e.g., "use the `bash` tool") that differ across harnesses. There is no mechanism to write harness-agnostic prompt templates that resolve tool names correctly at write time, forcing authors to maintain separate prompts per harness or accept incorrect names.

## What Changes

- Add `{{{tool <Name>}}}` Fue function syntax to template rendering, resolved at write time using the current `AgentHarness`
- Extend `Template` module with `renderWithHarness` and `renderFileWithHarness` functions that inject the `tool` function into the Fue data context
- Add `toolNameMap` lookup (DU case name string → `Tool` value) inside `MarkdownWriter`
- Update `writeMd` `Template` and `TemplateFile` branches to call harness-aware render functions
- Move `AgentHarness` type definition above the `Template` module to resolve forward-dependency

## Capabilities

### New Capabilities
- `harness-aware-template-rendering`: Template rendering that resolves `{{{tool <Name>}}}` to harness-correct tool name strings at write time

### Modified Capabilities
- `template-rendering`: Add harness-aware rendering functions alongside existing `renderInline`/`renderFile`; update write-time dispatch to use them

## Impact

- `src/FsAgent/Writers.fs`: `Template` module, `toolNameMap`, `writeMd` branches, `AgentHarness` position
- `tests/FsAgent.Tests/MarkdownWriterTests.fs`: New acceptance tests
- No AST changes; no DSL changes; no breaking changes to existing rendering behaviour
