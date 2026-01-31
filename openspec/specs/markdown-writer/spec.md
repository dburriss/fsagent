# markdown-writer Specification

## Purpose
TBD - created by archiving change add-markdown-writer. Update Purpose after archive.
## Requirements
### Requirement: Markdown Writer
The system SHALL provide a Markdown writer that converts the immutable Agent AST to a Markdown string with configurable options for heading renaming/formatting. Output MUST support multiple agent formats.

#### Scenario: Default write succeeds with imports resolved
- **WHEN** `writeMarkdown(agent, configure)` is called with no option changes
- **THEN** the writer returns a Markdown string
- **AND** headings use ATX style (`#`, `##`, ...)
- **AND** frontmatter is emitted at the top of the document by default according to the selected `outputFormat`
- **AND** imported data is always resolved and included
- **AND** imports with `wrapInCodeBlock=true` are wrapped in fenced code blocks
- **AND** imports with `wrapInCodeBlock=false` are embedded as raw content
- **AND** output order is deterministic per `agent-ast` traversal

#### Scenario: Import with wrapInCodeBlock true renders as code block
- **WHEN** an `Imported` node has `wrapInCodeBlock=true`
- **THEN** the content is wrapped in a fenced code block with language tag derived from DataFormat

#### Scenario: Import with wrapInCodeBlock false renders as raw
- **WHEN** an `Imported` node has `wrapInCodeBlock=false`
- **THEN** the content is embedded directly without code fences

#### Scenario: DisableCodeBlockWrapping forces raw output
- **WHEN** options specify `DisableCodeBlockWrapping=true`
- **THEN** all imports are embedded as raw content regardless of `wrapInCodeBlock` flag

#### Scenario: ATX-only headings
- **WHEN** headings are rendered
- **THEN** headings use ATX `#` prefixes for levels 1..N exclusively (no Setext)

#### Scenario: Heading rename map
- **WHEN** options provide `renameMap` with entries (e.g., `role -> "Role"`)
- **THEN** section headings use the mapped display names

#### Scenario: Global heading formatter (sensible default)
- **WHEN** options provide `headingFormatter: (name:string -> string)` or omit it
- **THEN** the formatted heading text is used after applying `renameMap` (default is identity)
- **AND** the writer validates that the formatter returns non-empty strings

#### Scenario: Deterministic output
- **WHEN** writing the same Agent AST with the same options
- **THEN** the exact Markdown output bytes are identical across runs

#### Scenario: Invalid option values
- **WHEN** options contain a malformed formatter
- **THEN** the writer returns an error (exception or error result) describing invalid configuration

### Requirement: Output Formats
The system MUST support selecting the agent output format via options with initial support for `opencode` and `copilot`.

#### Scenario: Select OpenCode format (default frontmatter)
- **WHEN** options specify `outputFormat=opencode`
- **THEN** headings use ATX style and common sections (e.g., Role, Objective, Instructions) are rendered consistently
- **AND** frontmatter is emitted by default at the top containing `description` only when available
- **AND** imported data may be presented following the pattern used in `knowledge/import-data.md` referencing rules from `knowledge/import-data.rules.json` when configured

#### Scenario: Select Copilot format with minimum frontmatter (default)
- **WHEN** options specify `outputFormat=copilot`
- **THEN** frontmatter includes at minimum `name` and `description` by default
- **AND** headings use ATX style and sections are rendered according to Copilot agent file conventions
- **AND** imported data is resolved and included per the `wrapInCodeBlock` flag on each import

#### Scenario: Missing required Copilot frontmatter
- **WHEN** `outputFormat=copilot` and required fields (`name`, `description`) are absent from AST frontmatter
- **THEN** the writer fails fast with a clear error describing missing fields

### Requirement: Writer Configuration API
The system SHALL expose a configuration API that uses a function receiving a mutable options object to set writer behavior, consistent with common .NET patterns.

#### Scenario: Configure via function mutation
- **WHEN** `writeMarkdown(agent, fun opts -> opts.DisableCodeBlockWrapping <- true; opts.outputFormat <- opencode)` is used
- **THEN** the resulting output reflects the configured options

#### Scenario: Sensible defaults
- **WHEN** `writeMarkdown(agent, configure)` is called without mutations
- **THEN** defaults apply: `outputFormat=Opencode`, ATX headings, `DisableCodeBlockWrapping=false`, frontmatter included, no renames, default heading formatter (identity), no footer, deterministic order

#### Scenario: Validation of options
- **WHEN** configuration sets conflicting or invalid combinations
- **THEN** the writer fails fast with a clear error message

### Requirement: Generated Footer Option
The system SHALL provide an optional footer generator function that receives writer context and returns a string to append to the output.

#### Scenario: Footer generator provided
- **WHEN** options specify `generatedFooter = Some (fun ctx -> ... )`
- **THEN** the writer appends the returned string at the end of the document

#### Scenario: No footer generator
- **WHEN** options omit the footer generator
- **THEN** no footer is appended

### Requirement: Code-block wrapping based on AST node flag
The system SHALL wrap imported content in fenced code blocks when the `Imported` node has `wrapInCodeBlock=true`, using format-derived language tags.

#### Scenario: JSON import with wrapInCodeBlock true
- **WHEN** an `Imported` node has `DataFormat = Json` and `wrapInCodeBlock = true`
- **THEN** the writer wraps content in ` ```json ... ``` `

#### Scenario: YAML import with wrapInCodeBlock true
- **WHEN** an `Imported` node has `DataFormat = Yaml` and `wrapInCodeBlock = true`
- **THEN** the writer wraps content in ` ```yaml ... ``` `

#### Scenario: TOON import with wrapInCodeBlock true
- **WHEN** an `Imported` node has `DataFormat = Toon` and `wrapInCodeBlock = true`
- **THEN** the writer wraps content in ` ```toon ... ``` `

#### Scenario: Unknown format with wrapInCodeBlock true
- **WHEN** an `Imported` node has `DataFormat = Unknown` and `wrapInCodeBlock = true`
- **THEN** the writer wraps content in ` ``` ... ``` ` (no language tag)

### Requirement: DisableCodeBlockWrapping option
The system SHALL provide a `DisableCodeBlockWrapping` option that forces all imports to render as raw content.

#### Scenario: DisableCodeBlockWrapping overrides wrapInCodeBlock
- **WHEN** `DisableCodeBlockWrapping=true` and an `Imported` node has `wrapInCodeBlock=true`
- **THEN** the content is embedded as raw (no code fences)

