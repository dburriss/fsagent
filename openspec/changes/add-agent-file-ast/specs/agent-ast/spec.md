# agent-ast Change Specification

## ADDED Requirements

### Requirement: Agent AST Core
The system SHALL provide an immutable, flavour-agnostic AST representing agent files, including frontmatter and structured sections.

#### Scenario: Construct agent with frontmatter and sections
- **WHEN** an agent is built via the DSL or directly via constructors
- **THEN** the AST root contains a `frontmatter: Map<string, obj>`
- **AND** section nodes exist for role, objective, instructions, examples, and other content
- **AND** the AST is immutable (no mutation during traversal or rendering)

#### Scenario: Deterministic node order
- **WHEN** nodes are added through the DSL or constructors
- **THEN** the resulting AST traversal order is deterministic

### Requirement: Imported Data Representation
Imported data MUST be stored in the AST as parsed objects along with the original declared format, enabling late-bound serialization by writers.

#### Scenario: Store parsed object with format
- **WHEN** a YAML or JSON file is imported
- **THEN** the AST holds the parsed object
- **AND** records the `DataFormat` (yaml|json)
- **AND** writers can choose TOON/YAML/JSON when rendering

### Requirement: Writer Independence
Writers MUST operate on the AST without mutating it and without depending on the DSL internals.

#### Scenario: Render via base writer
- **WHEN** a Markdown or JSON base writer renders the AST
- **THEN** rendering completes without modifying the AST
- **AND** flavour writers (OpenCode, Copilot, Claude) wrap the base writer to adjust frontmatter/layout only
