## ADDED Requirements

### Requirement: Template node type
The system SHALL extend the Node discriminated union with a Template case containing inline text.

#### Scenario: Create Template node
- **WHEN** system creates Template("Hello {{{name}}}")
- **THEN** system SHALL store the template text "Hello {{{name}}}" in the node

### Requirement: TemplateFile node type
The system SHALL extend the Node discriminated union with a TemplateFile case containing a file path.

#### Scenario: Create TemplateFile node
- **WHEN** system creates TemplateFile("templates/greeting.txt")
- **THEN** system SHALL store the path "templates/greeting.txt" in the node

### Requirement: Template module in Writers namespace
The system SHALL provide a Template module in FsAgent.Writers namespace with renderInline and renderFile functions.

#### Scenario: Module is accessible
- **WHEN** user opens FsAgent.Writers
- **THEN** system SHALL make Template.renderInline and Template.renderFile available

### Requirement: Fue integration
The system SHALL integrate Fue library (version 2.2.0) for template rendering using Mustache-compatible syntax.

#### Scenario: Fue dependency added
- **WHEN** project is built
- **THEN** system SHALL include Fue NuGet package reference

### Requirement: Template variable substitution
The system SHALL substitute variables in templates using Map<string, obj> for variable values.

#### Scenario: Render template with single variable
- **WHEN** user renders template "Hello {{{name}}}" with variables ["name" -> "Alice"]
- **THEN** system SHALL return "Hello Alice"

#### Scenario: Render template with multiple variables
- **WHEN** user renders template "{{{greeting}}} {{{name}}}" with variables ["greeting" -> "Hi"; "name" -> "Bob"]
- **THEN** system SHALL return "Hi Bob"

### Requirement: Inline template rendering
The system SHALL provide renderInline function that accepts text and variables and returns rendered string.

#### Scenario: Successful inline render
- **WHEN** user calls Template.renderInline "Text {{{var}}}" (Map ["var" -> "value"])
- **THEN** system SHALL return "Text value"

### Requirement: File template rendering
The system SHALL provide renderFile function that accepts file path and variables and returns rendered string.

#### Scenario: Successful file render
- **WHEN** file exists at path with content "{{{name}}}" AND user calls Template.renderFile path (Map ["name" -> "Test"])
- **THEN** system SHALL return "Test"

### Requirement: Template error handling for inline
The system SHALL catch rendering errors for inline templates and return error message in output.

#### Scenario: Inline render error
- **WHEN** template rendering throws exception with message "Invalid syntax"
- **THEN** system SHALL return "[Template error: Invalid syntax]"

### Requirement: Template error handling for files
The system SHALL catch file not found errors and return error message in output.

#### Scenario: File not found
- **WHEN** template file does not exist at path
- **THEN** system SHALL return "[Template file not found: {path}]"

#### Scenario: File render error
- **WHEN** file rendering throws exception with message "Parse error"
- **THEN** system SHALL return "[Template error: Parse error]"

### Requirement: Rendering at write time
The system SHALL render templates when writePrompt or writeAgent is called, not when template nodes are created.

#### Scenario: Template not rendered in builder
- **WHEN** user creates prompt with template operation
- **THEN** system SHALL store Template node without rendering

#### Scenario: Template rendered in writer
- **WHEN** writePrompt is called with TemplateVariables in options
- **THEN** system SHALL render all Template and TemplateFile nodes using provided variables

### Requirement: TemplateVariables in Options
The system SHALL add TemplateVariables field (Map<string, obj>) to MarkdownWriter Options type with default value of empty map.

#### Scenario: Default template variables
- **WHEN** user creates default options
- **THEN** system SHALL set TemplateVariables to empty Map

#### Scenario: Set template variables
- **WHEN** user configures options with TemplateVariables <- Map ["key" -> "value"]
- **THEN** system SHALL use these variables for template rendering
