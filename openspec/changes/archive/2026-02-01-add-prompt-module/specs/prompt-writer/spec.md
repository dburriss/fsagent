## ADDED Requirements

### Requirement: writePrompt function
The system SHALL provide writePrompt function in MarkdownWriter module that accepts Prompt and configuration function.

#### Scenario: Function signature
- **WHEN** user calls MarkdownWriter.writePrompt
- **THEN** system SHALL accept (Prompt -> (Options -> unit) -> string) signature

### Requirement: No frontmatter output for prompts
The system SHALL NOT output YAML frontmatter blocks when writing prompts to markdown format.

#### Scenario: Write prompt with metadata to markdown
- **WHEN** prompt has Frontmatter with "name" and "description" keys AND writePrompt is called with Md output type
- **THEN** system SHALL NOT include "---" frontmatter block in output

### Requirement: Frontmatter storage without serialization
The system SHALL store prompt metadata in Frontmatter map but not serialize it in markdown output.

#### Scenario: Metadata stored internally
- **WHEN** prompt has Frontmatter ["name" -> "test-prompt"]
- **THEN** system SHALL maintain this data in Prompt.Frontmatter map for programmatic access

### Requirement: Section rendering for prompts
The system SHALL render prompt Sections to markdown using same logic as agent sections.

#### Scenario: Write prompt sections
- **WHEN** prompt has role, objective, and instructions sections AND writePrompt is called
- **THEN** system SHALL output "# role", "# objective", "# instructions" headings with content

### Requirement: Template rendering in prompt output
The system SHALL render Template and TemplateFile nodes when writing prompts.

#### Scenario: Render inline template in prompt
- **WHEN** prompt has Template("Hello {{{name}}}") in Sections AND TemplateVariables includes "name" -> "Alice"
- **THEN** system SHALL output "Hello Alice" in rendered markdown

#### Scenario: Render file template in prompt
- **WHEN** prompt has TemplateFile("template.txt") AND file contains "{{{var}}}" AND TemplateVariables includes "var" -> "value"
- **THEN** system SHALL output "value" in rendered markdown

### Requirement: JSON output for prompts
The system SHALL serialize prompts to JSON format when OutputType is Json.

#### Scenario: Write prompt to JSON
- **WHEN** writePrompt is called with opts.OutputType <- Json
- **THEN** system SHALL return JSON string with frontmatter and sections fields

### Requirement: YAML output for prompts
The system SHALL serialize prompts to YAML format when OutputType is Yaml.

#### Scenario: Write prompt to YAML
- **WHEN** writePrompt is called with opts.OutputType <- Yaml
- **THEN** system SHALL return YAML string with frontmatter and sections fields

### Requirement: Options configurability
The system SHALL support all existing Options fields for prompt writing: OutputFormat, RenameMap, HeadingFormatter, TemplateVariables.

#### Scenario: Apply heading formatter to prompt
- **WHEN** opts.HeadingFormatter <- Some (fun s -> s.ToUpper()) AND writePrompt is called
- **THEN** system SHALL output section headings in uppercase

#### Scenario: Apply rename map to prompt sections
- **WHEN** opts.RenameMap <- Map ["role" -> "Agent Role"] AND writePrompt is called
- **THEN** system SHALL output "# Agent Role" instead of "# role"

### Requirement: WriterContext for prompts
The system SHALL create WriterContext with prompt name and description from Frontmatter when available.

#### Scenario: Context includes prompt metadata
- **WHEN** prompt Frontmatter has "name" and "description" AND writePrompt is called
- **THEN** system SHALL set AgentName and AgentDescription in WriterContext from these values
