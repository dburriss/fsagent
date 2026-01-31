## ADDED Requirements

### Requirement: Template node rendering in markdown
The system SHALL render Template nodes by substituting variables using TemplateVariables from Options.

#### Scenario: Render inline template in markdown
- **WHEN** Node is Template("Hello {{{name}}}") AND Options.TemplateVariables contains "name" -> "Alice"
- **THEN** output SHALL contain "Hello Alice" at template location in document

### Requirement: TemplateFile node rendering in markdown
The system SHALL render TemplateFile nodes by loading file content and substituting variables using TemplateVariables from Options.

#### Scenario: Render template file in markdown
- **WHEN** Node is TemplateFile("template.txt") AND file contains "{{{var}}}" AND Options.TemplateVariables contains "var" -> "value"
- **THEN** output SHALL contain "value" at template location in document

### Requirement: Template error handling in markdown output
The system SHALL catch template rendering errors and include error messages in markdown output instead of throwing exceptions.

#### Scenario: Template syntax error in output
- **WHEN** template rendering fails with error message
- **THEN** output SHALL contain "[Template error: {message}]" at template location

#### Scenario: Template file not found in output
- **WHEN** TemplateFile references non-existent file
- **THEN** output SHALL contain "[Template file not found: {path}]" at template location

### Requirement: TemplateVariables in Options
The system SHALL add TemplateVariables field (Map<string, obj>) to Options type with default empty map.

#### Scenario: Default template variables
- **WHEN** defaultOptions() is called
- **THEN** Options.TemplateVariables SHALL be empty Map

#### Scenario: Configure template variables
- **WHEN** user sets opts.TemplateVariables <- Map ["key" -> value]
- **THEN** template rendering SHALL use these variables for substitution

### Requirement: writePrompt function for Prompt type
The system SHALL provide writePrompt function that accepts Prompt and Options configuration function.

#### Scenario: Function signature
- **WHEN** writePrompt is called
- **THEN** signature SHALL be (Prompt -> (Options -> unit) -> string)

### Requirement: Prompt sections rendering
The system SHALL render Prompt.Sections using same markdown generation logic as Agent sections.

#### Scenario: Render prompt sections
- **WHEN** prompt contains role, objective, instructions sections
- **THEN** output SHALL include "# role", "# objective", "# instructions" headings with content

### Requirement: No frontmatter output for prompts
The system SHALL NOT output YAML frontmatter block when writing prompts, even though Prompt.Frontmatter may contain data.

#### Scenario: Prompt with metadata writes no frontmatter
- **WHEN** Prompt.Frontmatter contains "name" and "description" keys AND writePrompt is called with OutputType Md
- **THEN** output SHALL NOT include "---" YAML frontmatter block

### Requirement: Template rendering respects Options.OutputType
The system SHALL render templates for all OutputType values: Md, Json, Yaml.

#### Scenario: Template in JSON output
- **WHEN** OutputType is Json AND prompt contains Template node
- **THEN** rendered template content SHALL appear in JSON structure

#### Scenario: Template in YAML output
- **WHEN** OutputType is Yaml AND prompt contains Template node
- **THEN** rendered template content SHALL appear in YAML structure

### Requirement: Namespace relocation for MarkdownWriter
The system SHALL provide MarkdownWriter module in FsAgent.Writers namespace instead of FsAgent module.

#### Scenario: Access writer from Writers namespace
- **WHEN** user opens FsAgent.Writers
- **THEN** MarkdownWriter module SHALL be available

### Requirement: Template module in Writers namespace
The system SHALL provide Template module in FsAgent.Writers namespace with renderInline and renderFile functions.

#### Scenario: Access Template module
- **WHEN** user opens FsAgent.Writers
- **THEN** Template.renderInline and Template.renderFile SHALL be available
