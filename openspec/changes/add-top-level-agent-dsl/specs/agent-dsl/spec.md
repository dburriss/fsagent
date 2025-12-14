## ADDED Requirements

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
