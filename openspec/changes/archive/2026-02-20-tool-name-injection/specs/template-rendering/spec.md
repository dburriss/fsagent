## MODIFIED Requirements

### Requirement: Template module in Writers namespace
The system SHALL provide a Template module in FsAgent.Writers namespace with renderInline, renderFile, renderWithHarness, and renderFileWithHarness functions.

#### Scenario: Module is accessible
- **WHEN** user opens FsAgent.Writers
- **THEN** system SHALL make Template.renderInline, Template.renderFile, Template.renderWithHarness, and Template.renderFileWithHarness available

### Requirement: Rendering at write time
The system SHALL render templates when renderPrompt or renderAgent is called, not when template nodes are created. When a harness is available, the system SHALL use harness-aware rendering for Template and TemplateFile nodes.

#### Scenario: Template not rendered in builder
- **WHEN** user creates prompt with template operation
- **THEN** system SHALL store Template node without rendering

#### Scenario: Template rendered with harness in writer
- **WHEN** renderPrompt is called with a harness set in the format context and TemplateVariables in options
- **THEN** system SHALL render all Template and TemplateFile nodes using renderWithHarness/renderFileWithHarness with the current harness

## ADDED Requirements

### Requirement: AgentHarness defined before Template module
The system SHALL define the AgentHarness discriminated union before the Template module in Writers.fs.

#### Scenario: AgentHarness available as Template module parameter type
- **WHEN** the project is compiled
- **THEN** system SHALL resolve AgentHarness as a valid type within the Template module's function signatures without forward-reference errors
