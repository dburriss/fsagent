# agent-ast Change Specification

## ADDED Requirements

### Requirement: Agent AST Core
The system SHALL provide an immutable, flavour-agnostic AST representing agent files, including frontmatter and structured sections.

#### Scenario: Construct agent with frontmatter and sections
- **WHEN** an agent is built via direct constructors
- **THEN** the AST root contains a `frontmatter: Map<string, obj>`
- **AND** frontmatter supports nested maps/arrays
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

### Requirement: AST Construction API (core sections and examples)
The system SHALL expose pure constructor functions in the `AST` module for common section types `role`, `objective`, `instructions`, `context`, and example collections via `examples`.

#### Scenario: Create role section via constructor
- **WHEN** `role "You are an Assistant"` is used
- **THEN** the AST contains a `Section` node with `name="role"` and text `"You are an Assistant"`
- **AND** the node is immutable and order deterministic

#### Scenario: Create objective section via constructor
- **WHEN** `objective "blah..."` is used
- **THEN** the AST contains a `Section` node with `name="objective"` and text `"blah..."`
- **AND** the node is immutable and order deterministic

#### Scenario: Create instructions section via constructor
- **WHEN** `instructions "Follow these steps..."` is used
- **THEN** the AST contains a `Section` node with `name="instructions"` and text `"Follow these steps..."`
- **AND** the node is immutable and order deterministic

#### Scenario: Create context section via constructor
- **WHEN** `context "Project-specific context..."` is used
- **THEN** the AST contains a `Section` node with `name="context"` and text `"Project-specific context..."`
- **AND** the node is immutable and order deterministic

#### Scenario: Create examples list via constructor
- **WHEN** `examples [ example [ output "Final answer..."; instructions "Steps used"; context "Relevant notes" ]; example [ output "Another answer" ] ]` is used
- **THEN** the AST contains a `Section` node with `name="examples"` whose items hold each `example` Section in order
- **AND** each `example` is a `Section` holding a list of `Section` nodes (e.g., `output`, `instructions`, `context`)
- **AND** the nodes are immutable and traversal order deterministic

#### Scenario: Function signatures
- **WHEN** reviewing the API
- **THEN** signatures are `role: string -> Section`, `objective: string -> Section`, `instructions: string -> Section`, `context: string -> Section`, `output: string -> Section`, `example: Section list -> Section`, and `examples: Section list -> Section` exported from the `AST` module
