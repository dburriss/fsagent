# agent-dsl Specification

## Purpose
The Agent DSL provides a top-level F# computation expression (CE) `agent { ... }` that constructs an immutable Agent AST. The DSL remains flavour-agnostic, focusing on ergonomics for common agent authoring patterns while delegating writer-specific concerns to the output layer.

## Requirements

### Requirement: Top-level CE `agent { ... }`
The system SHALL provide a computation expression `agent { ... }` that yields an `Agent` AST node.

#### Scenario: Construct agent with sections
- **WHEN** `agent { role "You are an assistant"; objective "Help users" }` is evaluated
- **THEN** the result is an `Agent` AST with sections for "role" and "objective"
- **AND** sections are ordered deterministically as declared

### Requirement: Frontmatter builder `meta { ... }`
The CE SHALL support a `meta { ... }` block that builds a frontmatter `Map<string, obj>`.

#### Scenario: Add string key-value
- **WHEN** `meta { kv "description" "An agent" }` is used
- **THEN** frontmatter contains `("description", "An agent" :> obj)`

#### Scenario: Add list key-value
- **WHEN** `meta { kvList "tools" ["tool1"; "tool2"] }` is used
- **THEN** frontmatter contains `("tools", ["tool1"; "tool2"] :> obj)`

#### Scenario: Add object key-value
- **WHEN** `meta { kvObj "model" (Map.ofList [("name", "gpt-4" :> obj)]) }` is used
- **THEN** frontmatter contains `("model", Map(...) :> obj)`

#### Scenario: Add list of objects
- **WHEN** `meta { kvListObj "servers" [Map.ofList [("name", "s1" :> obj)] :> obj] }` is used
- **THEN** frontmatter contains `("servers", [Map(...)] :> obj)`

### Requirement: Core section operations
The CE SHALL support operations for core sections: `role`, `objective`, `instructions`, `context`, `output`.

#### Scenario: Add role section
- **WHEN** `role "You are..."` is used in CE
- **THEN** a Section node with name "role" is added

(Similar for others)

### Requirement: Examples section
The CE SHALL support `examples [ example "title" "content"; ... ]`

#### Scenario: Add examples
- **WHEN** `examples [ example "Q" "A" ]` is used
- **THEN** a Section "examples" containing example Sections is added

### Requirement: Generic section
The CE SHALL support `section "name" "content"` for ad hoc sections.

### Requirement: Import operation
The CE SHALL support `import "path"` that creates an import reference with inferred format.

#### Scenario: Import with extension
- **WHEN** `import "data.yml"` is used
- **THEN** an import reference with path "data.yml" and format Yaml is added

### Requirement: Deterministic ordering
Frontmatter SHALL appear first, followed by sections in declaration order.

### Requirement: Purity
The DSL SHALL be pure; no I/O performed during construction.