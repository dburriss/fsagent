# markdown-writer Specification

## Purpose
TBD - created by archiving change add-markdown-writer. Update Purpose after archive.
## Requirements
### Requirement: Markdown Writer
The system SHALL provide a Markdown writer that converts the immutable Agent AST to a Markdown string with configurable options for imported data inclusion and heading renaming/formatting. Output MUST support multiple agent formats.

#### Scenario: Default write succeeds
- **WHEN** `writeMarkdown(agent, configure)` is called with no option changes
- **THEN** the writer returns a Markdown string
- **AND** headings use ATX style (`#`, `##`, ...)
- **AND** frontmatter is emitted at the top of the document by default according to the selected `outputFormat`
- **AND** imported data is not embedded (`importInclusion=none`)
- **AND** output order is deterministic per `agent-ast` traversal

#### Scenario: Raw imported data inclusion
- **WHEN** options specify `importInclusion=raw`
- **THEN** imported data is inserted as raw transformed content without automatic code fences
- **AND** authors can explicitly wrap content in code blocks within the AST sections if desired

#### Scenario: No imported data inclusion
- **WHEN** options specify `importInclusion=none`
- **THEN** the writer does not embed imported data content in the output

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
- **WHEN** options contain unsupported `importInclusion` or a malformed formatter
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
- **AND** imported data handling follows the configured inclusion (`raw` or `none`)

#### Scenario: Missing required Copilot frontmatter
- **WHEN** `outputFormat=copilot` and required fields (`name`, `description`) are absent from AST frontmatter
- **THEN** the writer fails fast with a clear error describing missing fields

### Requirement: Writer Configuration API
The system SHALL expose a configuration API that uses a function receiving a mutable options object to set writer behavior, consistent with common .NET patterns.

#### Scenario: Configure via function mutation
- **WHEN** `writeMarkdown(agent, fun opts -> opts.importInclusion <- raw; opts.outputFormat <- opencode)` is used
- **THEN** the resulting output reflects the configured options

#### Scenario: Sensible defaults
- **WHEN** `writeMarkdown(agent, configure)` is called without mutations
- **THEN** defaults apply: `outputFormat=opencode`, ATX headings, `importInclusion=none`, frontmatter included, no renames, default heading formatter (identity), no footer, deterministic order

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

