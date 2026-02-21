## Why

Many modern LLMs parse structured prompts more reliably when sections use XML tags (`<role>...</role>`) rather than Markdown headings (`## role`). Callers need an opt-in rendering mode that works at the writer level without any changes to the DSL or AST.

## What Changes

- Add `SectionStyle` discriminated union (`Markdown` | `Xml`) to `Writers.fs`
- Add `SectionStyle` field to `AgentWriter.Options` record, defaulting to `Markdown`
- Update `writeNode` in both `renderMd` and `renderSkill` to branch on `SectionStyle`:
  - `Markdown`: existing behaviour, unchanged
  - `Xml`: render `Section` nodes as `<name>...</name>` tags instead of `## name` headings
- `RenameMap` and `HeadingFormatter` continue to apply to the tag name in XML mode

## Capabilities

### New Capabilities

- `xml-section-style`: A `SectionStyle` rendering option on `AgentWriter.Options` that switches `Section` node output from Markdown headings to XML tags

### Modified Capabilities

## Impact

- `src/FsAgent/Writers.fs` — new DU, new `Options` field, updated `writeNode` in two render functions
- `tests/FsAgent.Tests/AgentWriterTests.fs` — new tests for XML output
- `CHANGELOG.md` — entry for the new option
