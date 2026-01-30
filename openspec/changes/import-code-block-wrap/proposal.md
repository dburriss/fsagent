## Why

The current `import` operation embeds file content as raw text, which works for some formats but loses semantic context. When importing structured data files (TOON, YAML, JSON), wrapping content in a fenced code block with the appropriate language tag (e.g., ` ```json `) preserves format hints for downstream consumers (AI agents, syntax highlighters).

## What Changes

- **BREAKING**: Rename current DSL `import` operation to `importRaw` (embeds content without wrapping)
- Add new `import` operation that wraps embedded content in a fenced code block using the inferred format as the language tag
- Add new `ImportInclusion` mode `IncludeCodeBlock` for the writer to handle code-fenced embedding

## Capabilities

### New Capabilities

None - this modifies existing import behavior.

### Modified Capabilities

- `agent-dsl`: Rename `import` to `importRaw`, add new `import` that produces code-block-wrapped output
- `markdown-writer`: Add `IncludeCodeBlock` import inclusion mode that wraps content in fenced code blocks with format-derived language tags

## Impact

- `src/FsAgent/Library.fs`: DSL and MarkdownWriter modules
- `tests/FsAgent.Tests/`: New and updated tests for import operations
- Existing code using `import` will need to change to `importRaw` to preserve current behavior (breaking change)
