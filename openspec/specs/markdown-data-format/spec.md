# Spec: Markdown DataFormat Variant

## Purpose

Defines the `Markdown` variant of the `DataFormat` discriminated union and how `inferFormat` maps `.md` and `.markdown` file extensions to it, along with Markdown writer rendering behaviour.

## Requirements

### Requirement: Markdown DataFormat variant
The `DataFormat` discriminated union SHALL include a `Markdown` variant. The `inferFormat` function SHALL map `.md` and `.markdown` file extensions to `Markdown`.

#### Scenario: inferFormat maps .md to Markdown
- **WHEN** `inferFormat` is called with a path ending in `.md`
- **THEN** the result is `DataFormat.Markdown`

#### Scenario: inferFormat maps .markdown to Markdown
- **WHEN** `inferFormat` is called with a path ending in `.markdown`
- **THEN** the result is `DataFormat.Markdown`

#### Scenario: Markdown format renders as raw text without code block
- **WHEN** an `Imported(path, Markdown, false)` node is written by the Markdown writer
- **THEN** the file content is emitted as raw text with no fenced code block

#### Scenario: Markdown format with wrapInCodeBlock uses markdown language tag
- **WHEN** an `Imported(path, Markdown, true)` node is written by the Markdown writer
- **THEN** the output is wrapped in a fenced code block with language tag `markdown`

#### Scenario: Unknown extensions still produce Unknown format
- **WHEN** `inferFormat` is called with a path ending in an unrecognised extension (e.g., `.txt`)
- **THEN** the result is `DataFormat.Unknown`
