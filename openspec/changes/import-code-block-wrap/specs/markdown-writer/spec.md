## MODIFIED Requirements

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

#### Scenario: Code-block imported data inclusion
- **WHEN** options specify `importInclusion=codeBlock`
- **THEN** imported data is wrapped in a fenced code block
- **AND** the code fence language tag is derived from the import's DataFormat (yaml, json, toon, or empty for Unknown)

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

## ADDED Requirements

### Requirement: Code-block import inclusion mode
The system SHALL support an `importInclusion=codeBlock` option that wraps imported content in fenced code blocks with format-derived language tags.

#### Scenario: JSON import with code-block inclusion
- **WHEN** an import reference has `DataFormat = Json` and `importInclusion=codeBlock`
- **THEN** the writer wraps content in ` ```json ... ``` `

#### Scenario: YAML import with code-block inclusion
- **WHEN** an import reference has `DataFormat = Yaml` and `importInclusion=codeBlock`
- **THEN** the writer wraps content in ` ```yaml ... ``` `

#### Scenario: TOON import with code-block inclusion
- **WHEN** an import reference has `DataFormat = Toon` and `importInclusion=codeBlock`
- **THEN** the writer wraps content in ` ```toon ... ``` `

#### Scenario: Unknown format with code-block inclusion
- **WHEN** an import reference has `DataFormat = Unknown` and `importInclusion=codeBlock`
- **THEN** the writer wraps content in ` ``` ... ``` ` (no language tag)
