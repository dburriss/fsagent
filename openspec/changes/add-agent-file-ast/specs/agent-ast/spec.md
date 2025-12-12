# agent-ast Change Specification

## ADDED Requirements

### Requirement: Agent AST Core
The system SHALL provide an immutable, flavour-agnostic AST representing agent files, including frontmatter and structured sections.

#### Scenario: Construct agent with frontmatter and sections
- **WHEN** an agent is built via direct constructors
- **THEN** the AST root contains a `frontmatter: Map<string, obj>`
- **AND** section nodes exist for role, objective, instructions, examples, context, and other content
- **AND** the AST is immutable (no mutation during traversal)

#### Scenario: Deterministic node order
- **WHEN** nodes are added through direct constructors
- **THEN** the resulting AST traversal order is deterministic

### Requirement: Imported Data References (Path-Only)
Imported data MUST be represented in the AST as references (path + declared format), without parsed objects held inside the AST.

#### Scenario: Store `sourcePath` with declared format
- **WHEN** a YAML, JSON, or TOON file is referenced for import
- **THEN** the AST holds the `sourcePath`
- **AND** records the `DataFormat` (yaml|json|toon)
- **AND** no parsed object is stored in the AST
