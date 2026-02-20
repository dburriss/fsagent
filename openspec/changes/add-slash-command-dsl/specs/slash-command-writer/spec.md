## ADDED Requirements

### Requirement: writeCommand renders description-only frontmatter
The system SHALL provide `MarkdownWriter.writeCommand : SlashCommand -> (Options -> unit) -> string` that renders a YAML front block containing only the `description` key.

#### Scenario: description appears in frontmatter
- **WHEN** `writeCommand` is called with a command whose `Description = "Does a thing"`
- **THEN** the output begins with `---\ndescription: Does a thing\n---`

#### Scenario: name is absent from frontmatter
- **WHEN** `writeCommand` is called with a command whose `Name = "my-cmd"`
- **THEN** the output does NOT contain `name:` in the YAML front block

#### Scenario: empty description renders empty frontmatter value
- **WHEN** `writeCommand` is called with `Description = ""`
- **THEN** the output contains `description: ` with an empty value (or omits the key — consistent with existing `writeAgent` behaviour for empty strings)

### Requirement: writeCommand renders sections as Markdown headings
Sections in a `SlashCommand` SHALL be rendered to Markdown headings and body text using the same rules as `writeAgent`.

#### Scenario: instructions section renders as heading
- **WHEN** a command has an `instructions` section with text "Step 1"
- **THEN** the output contains `# instructions` followed by `Step 1`

#### Scenario: custom section renders as heading
- **WHEN** a command has `section "notes" "some note"`
- **THEN** the output contains `# notes` followed by `some note`

### Requirement: writeCommand resolves template variables per harness
Template nodes in a `SlashCommand` SHALL be rendered by `Template.renderWithHarness` using the harness specified in `Options.OutputFormat`, identical to `writeAgent`.

#### Scenario: template renders with Opencode harness
- **WHEN** a command contains `Template "Use {{{tool Bash}}}"` and `OutputFormat = Opencode`
- **THEN** the output contains the Opencode-resolved form of `{{{tool Bash}}}`

#### Scenario: template renders identically for all three harnesses
- **WHEN** `writeCommand` is called with `Opencode`, `Copilot`, and `ClaudeCode` options on the same command
- **THEN** all three outputs are identical (no harness-conditional rendering today)

### Requirement: writeCommand accepts OutputType option
`writeCommand` SHALL respect `Options.OutputType` (`Md`, `Json`, `Yaml`) following the same dispatch as `writeAgent`.

#### Scenario: default output type is Markdown
- **WHEN** `writeCommand` is called with no `OutputType` override
- **THEN** the output is a Markdown string (starts with `---`)
