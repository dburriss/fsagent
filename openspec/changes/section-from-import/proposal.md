## Why

Large inline triple-quoted strings in `AGENTS.fsx` and skill `.fsx` files are hard to read, diff, and maintain. Section content should live in standalone files editable in isolation, with the DSL referencing them by path.

## What Changes

- Add `Markdown` variant to the `DataFormat` discriminated union in `AST.fs`
- Update `inferFormat` to map `.md` and `.markdown` extensions to `Markdown` (currently falls through to `Unknown`)
- Add `sectionFrom` custom operation to all 4 CE builders: `Prompt.fs`, `Agent.fs`, `Skill.fs`, `Command.fs`
- Ensure the writer handles `Markdown` format as raw text (no code-block wrapping by default)
- Extract inline section strings from `AGENTS.fsx` into `knowledge/agents/*.md` files
- Rewrite `AGENTS.fsx` to use `sectionFrom` instead of inline triple-quoted strings

## Capabilities

### New Capabilities
- `section-from`: `sectionFrom` DSL operation that creates a `Section` whose body is loaded from an external file at write time
- `markdown-data-format`: `Markdown` as a first-class `DataFormat` variant so `.md` files are handled explicitly rather than falling through to `Unknown`

### Modified Capabilities

## Impact

- `src/AST.fs` — new `Markdown` DU case, updated `inferFormat`
- `src/Writers.fs` — `resolveImportedContent` / `writeNode` must handle `Markdown` (raw text, language tag `"markdown"` if code-block wrap is requested)
- `src/Prompt.fs`, `src/Agent.fs`, `src/Skill.fs`, `src/Command.fs` — new `sectionFrom` custom operation
- `AGENTS.fsx` — rewritten to use `sectionFrom`
- `knowledge/agents/` — new directory with one `.md` file per section
- `tests/` — new A-category and C-category tests
