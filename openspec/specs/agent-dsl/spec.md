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

#### Scenario: Add generic section
- **WHEN** `section "notes" "text"` is used in CE
- **THEN** a Section "notes" with content "text" is added

### Requirement: Import operation
The CE SHALL support `import "path"` that creates an import reference with inferred format.

#### Scenario: Import with extension
- **WHEN** `import "data.yml"` is used
- **THEN** an import reference with path "data.yml" and format Yaml is added

### Requirement: Deterministic ordering
Frontmatter SHALL appear first, followed by sections in declaration order.

#### Scenario: Frontmatter before sections
- **WHEN** agent includes meta and multiple sections
- **THEN** frontmatter is first; sections follow declaration order

### Requirement: Purity
The DSL SHALL be pure; no I/O performed during construction.

#### Scenario: No I/O during construction
- **WHEN** agent includes import references
- **THEN** AST records references; no filesystem/network I/O occurs

### Requirement: Agent DSL Computation Expression
The system SHALL provide a top-level F# computation expression `agent { ... }` that constructs an Agent AST root in a deterministic, immutable manner.

#### Scenario: Build agent via CE
- **WHEN** an agent is defined using `agent { role "You are..."; objective "..."; instructions "..." }`
- **THEN** the CE yields an Agent AST root
- **AND** the AST preserves section order as declared
- **AND** the AST remains immutable

### Requirement: Frontmatter Builder in DSL
The system SHALL provide a flavour-agnostic frontmatter builder usable inside the CE to populate optional metadata without enforcing required keys.

#### Scenario: Define common frontmatter
- **WHEN** `meta { description "Review code"; model "gpt-4.1"; temperature 0.2; tools ["grep"; "bash"]; name "readme-writer" }` is used inside `agent { ... }`
- **THEN** the resulting AST root contains a frontmatter map with keys `description`, `model`, `temperature`, `tools` (strings), and optional `name`
- **AND** nested arrays/maps are supported (tools list, future nested configs)
- **AND** no flavour-specific enforcement occurs in the DSL (writers interpret keys per flavour)

#### Scenario: Add typed list/object metadata
- **WHEN** `meta { kv "x-custom" "value"; kvList "args" ["--arg1"; "--arg2"]; kvObj "env" (map [ "ENV_VAR_NAME", "${{ secrets.MY_SECRET }}" ]) ; kvListObj "tools" [ box "*" ] }`
- **THEN** the AST frontmatter contains these entries with lists and nested maps represented deterministically

### Requirement: Ad Hoc Metadata and Sections
The system SHALL allow adding arbitrary metadata keys to frontmatter and arbitrary named sections to avoid restrictiveness (e.g., MCP-related configuration and custom content).

#### Scenario: Add ad hoc metadata
- **WHEN** `meta { kv "mcp-servers" "configured-elsewhere"; kv "x-note" "value" }` is used
- **THEN** the AST frontmatter contains those keys and values without validation by the DSL

#### Scenario: Add ad hoc section
- **WHEN** `section "rules" "Follow stratified design; keep AST pure."` is used
- **THEN** the AST contains a `Section` node with `name="rules"` and the provided text content

### Requirement: Section Operations in DSL
The system SHALL provide DSL operations for core sections: `role`, `objective`, `instructions`, `context`, `output`, `example`, and `examples`.

#### Scenario: Create role/objective/instructions/context/output sections
- **WHEN** `role "You are an Assistant"`, `objective "Upgrade project"`, `instructions "Follow steps"`, `context "Project details"`, and `output "Summarize changes"` are used
- **THEN** corresponding Section nodes are added to the AST in the declared order

#### Scenario: Create examples collection
- **WHEN** `examples [ example "Title" "Content"; example "Another" "Details" ]` is used
- **THEN** the AST contains an `examples` Section with ordered `example` Section children

### Requirement: Imported Data References via DSL
The system SHALL allow declaring imported data references within the DSL that become path references in the AST with format inferred from filename.

#### Scenario: Declare import reference with inferred format
- **WHEN** `import "knowledge/import-data.rules.json"` is used inside the CE
- **THEN** the AST holds a reference node with `sourcePath = "knowledge/import-data.rules.json"` and `DataFormat = json` inferred from the `.json` extension
- **AND** no parsed object is embedded in the AST (writers resolve at write time)

### Requirement: Configure MCP via typed metadata
The system SHALL support expressing `mcp-servers` frontmatter using typed list/object helpers.

#### Scenario: Add `mcp-servers` with nested config
- **WHEN** `meta { kvObj "mcp-servers" (map [ "custom-mcp", boxMap (map [ "type", box "local"; "command", box "some-command"; "args", boxList [ box "--arg1"; box "--arg2" ]; "tools", boxList [ box "*" ]; "env", boxMap (map [ "ENV_VAR_NAME", box "${{ secrets.MY_SECRET }}" ]) ]) ]) }` is used
- **THEN** the AST frontmatter contains a nested map for `mcp-servers` compatible with writer serialization

