# agent-ast Change Specification

## ADDED Requirements

### Requirement: Agent AST Core
The system SHALL provide an immutable, flavour-agnostic AST representing agent files, including frontmatter and structured sections.

#### Scenario: Construct agent with frontmatter and sections
- **WHEN** an agent is built via the DSL or directly via constructors
- **THEN** the AST root contains a `frontmatter: Map<string, obj>`
- **AND** section nodes exist for role, objective, instructions, examples, context, and other content
- **AND** the AST is immutable (no mutation during traversal or rendering)
- **AND** writers do not mutate the AST

#### Scenario: Deterministic node order
- **WHEN** nodes are added through the DSL or constructors
- **THEN** the resulting AST traversal order is deterministic

### Requirement: Imported Data References (Path-Only)
Imported data MUST be represented in the AST as references (path + declared format), without parsed objects held inside the AST.

#### Scenario: Store `sourcePath` with declared format
- **WHEN** a YAML or JSON file is imported
- **THEN** the AST holds the `sourcePath`
- **AND** records the `DataFormat` (yaml|json|toon)
- **AND** no parsed object is stored in the AST

#### Scenario: Writers resolve import at write time
- **WHEN** a writer renders an agent with import references
- **THEN** the writer reads the referenced file via low-level IO/parsing modules
- **AND** the writer selects the final serialization format (TOON/YAML/JSON) at write time

### Requirement: Writer IO Responsibilities
Writers MAY perform IO to resolve import references and produce outputs, but MUST delegate the actual reading/parsing to lower-level modules and MUST NOT mutate the AST.

#### Scenario: IO via lower-level modules
- **WHEN** a writer encounters an import reference in the AST
- **THEN** it uses low-level import/parsing modules to load and parse the file
- **AND** handles missing file or parse errors predictably (e.g., fail with a clear message or emit a placeholder)

### Requirement: Writer Independence
Writers MUST operate on the AST without relying on DSL internals.

#### Scenario: Render via base writer
- **WHEN** a Markdown or JSON base writer renders the AST
- **THEN** rendering completes without modifying the AST
- **AND** flavour writers (OpenCode, Copilot, Claude) wrap the base writer to adjust frontmatter/layout only
