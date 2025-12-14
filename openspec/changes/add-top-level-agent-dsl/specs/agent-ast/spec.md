## MODIFIED Requirements

### Requirement: AST Construction API (core sections and examples)
The system SHALL expose pure constructor functions in the `AST` module for common section types `role`, `objective`, `instructions`, `context`, and example collections via `examples`, and SHALL provide helper constructors for frontmatter entries and imported data references to support a stratified top-level DSL.

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

#### Scenario: Construct frontmatter entries via helpers
- **WHEN** frontmatter is built via helpers (e.g., `fmStr "description" "Review code"`, `fmNum "temperature" 0.2`, `fmList "tools" [ box "grep"; box "bash" ]`, `fmStr "model" "gpt-4.1"`, optional `fmStr "name" "readme-writer"`)
- **THEN** the AST root contains a `frontmatter: Map<string, obj>` composed from those entries
- **AND** nested maps/arrays are supported via `fmMap` and `fmList`
- **AND** helpers are pure and flavour-agnostic

#### Scenario: Build nested frontmatter via helpers (MCP servers)
- **WHEN** `fmMap "mcp-servers" (map [ "custom-mcp", boxMap (map [ "type", box "local"; "command", box "some-command"; "args", boxList [ box "--arg1"; box "--arg2" ]; "tools", boxList [ box "*" ]; "env", boxMap (map [ "ENV_VAR_NAME", box "${{ secrets.MY_SECRET }}" ]) ]) ])` is used
- **THEN** the AST frontmatter holds a nested map for `mcp-servers` and preserves deterministic ordering

#### Scenario: Create import reference via constructor with inferred format
- **WHEN** `importRef "knowledge/import-data.rules.json"` is used
- **THEN** the AST contains a reference node holding `sourcePath = "knowledge/import-data.rules.json"` and `DataFormat = json` inferred from the filename extension
- **AND** no parsed object is stored within the AST (writers resolve at write time)

#### Scenario: Function signatures
- **WHEN** reviewing the API
- **THEN** signatures are `role: string -> Section`, `objective: string -> Section`, `instructions: string -> Section`, `context: string -> Section`, `output: string -> Section`, `example: string -> string -> Section`, `examples: Section list -> Section`, frontmatter helpers `fmStr: string -> string -> FrontmatterEntry`, `fmNum: string -> float -> FrontmatterEntry`, `fmBool: string -> bool -> FrontmatterEntry`, `fmList: string -> obj list -> FrontmatterEntry`, `fmMap: string -> Map<string, obj> -> FrontmatterEntry`, and `importRef: sourcePath:string -> Section` (format inferred from filename) exported from the `AST` module
