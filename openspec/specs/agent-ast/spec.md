# agent-ast Specification

## Purpose
TBD - created by archiving change add-agent-file-ast. Update Purpose after archive.
## Requirements
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
- **WHEN** `examples [ example "Example 1 Title" "Example 1 content"; example "Example 2 Title" "Example 2 content" ]` is used
- **THEN** the AST contains a `Section` node with `name="examples"` whose items hold each `example` Section in order
- **AND** each `example` is a `Section` with `name="example"` holding a list of `Text` nodes for title and content
- **AND** the nodes are immutable and traversal order deterministic

#### Scenario: Function signatures
- **WHEN** reviewing the API
- **THEN** signatures are `role: string -> Section`, `objective: string -> Section`, `instructions: string -> Section`, `context: string -> Section`, `output: string -> Section`, `example: string -> string -> Section`, and `examples: Section list -> Section` exported from the `AST` module

### Requirement: Frontmatter Helper Constructors
The system SHALL provide helper constructors for frontmatter values to support DSL ergonomics.

#### Scenario: Construct string value
- **WHEN** `fmStr "value"` is used
- **THEN** it returns `"value" :> obj`

#### Scenario: Construct numeric value
- **WHEN** `fmNum 42.0` is used
- **THEN** it returns `42.0 :> obj`

#### Scenario: Construct boolean value
- **WHEN** `fmBool true` is used
- **THEN** it returns `true :> obj`

#### Scenario: Construct list value
- **WHEN** `fmList ["a"; "b"]` is used
- **THEN** it returns `["a"; "b"] :> obj`

#### Scenario: Construct map value
- **WHEN** `fmMap (Map.ofList [("k", "v" :> obj)])` is used
- **THEN** it returns `Map(...) :> obj`

### Requirement: Import Reference with Format Inference
The system SHALL provide `importRef` that infers format from filename extension.

#### Scenario: Infer YAML format
- **WHEN** `importRef "data.yml"` is used
- **THEN** it returns an import reference with path "data.yml" and format Yaml

#### Scenario: Infer JSON format
- **WHEN** `importRef "data.json"` is used
- **THEN** it returns an import reference with path "data.json" and format Json

#### Scenario: Infer TOON format
- **WHEN** `importRef "data.toon"` is used
- **THEN** it returns an import reference with path "data.toon" and format Toon

#### Scenario: Unknown extension
- **WHEN** `importRef "data.txt"` is used
- **THEN** it returns an import reference with path "data.txt" and format Unknown

