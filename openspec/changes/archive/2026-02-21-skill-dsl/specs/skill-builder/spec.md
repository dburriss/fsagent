## ADDED Requirements

### Requirement: Skill record type
The system SHALL provide a `Skill` record type in the `FsAgent.Skills` namespace with `Frontmatter: Map<string, obj>` and `Sections: Node list` fields, consistent with the `Agent` record structure.

#### Scenario: Default empty skill
- **WHEN** `Skill.empty` is accessed
- **THEN** it returns a `Skill` with an empty `Frontmatter` map and empty `Sections` list

### Requirement: Skill computation expression builder
The system SHALL provide a `skill { ... }` computation expression in an `[<AutoOpen>]` module within `FsAgent.Skills`, exposing a value named `skill` of type `SkillBuilder`.

#### Scenario: Build skill with no operations
- **WHEN** `skill { () }` is evaluated
- **THEN** the result equals `Skill.empty`

### Requirement: Frontmatter custom operations
The `SkillBuilder` SHALL support the following custom operations that write into `Frontmatter`:
- `name` (string) → key `"name"`
- `description` (string) → key `"description"`
- `license` (string) → key `"license"`
- `compatibility` (string) → key `"compatibility"`
- `metadata` (`Map<string, obj>`) → key `"metadata"`

#### Scenario: Set name
- **WHEN** `skill { name "my-skill" }` is evaluated
- **THEN** `Frontmatter` contains `"name" → "my-skill"`

#### Scenario: Set description
- **WHEN** `skill { description "Does X" }` is evaluated
- **THEN** `Frontmatter` contains `"description" → "Does X"`

#### Scenario: Set license
- **WHEN** `skill { license "MIT" }` is evaluated
- **THEN** `Frontmatter` contains `"license" → "MIT"`

#### Scenario: Set compatibility
- **WHEN** `skill { compatibility "Requires openspec CLI." }` is evaluated
- **THEN** `Frontmatter` contains `"compatibility" → "Requires openspec CLI."`

#### Scenario: Set metadata map
- **WHEN** `skill { metadata (Map.ofList ["author", box "alice"; "version", box "1.0"]) }` is evaluated
- **THEN** `Frontmatter` contains `"metadata" → Map ["author","alice"; "version","1.0"]`

### Requirement: Section content operations
The `SkillBuilder` SHALL support the following custom operations that append to `Sections`:
- `section` (name: string, content: string) → appends `Section(name, [Text content])`
- `prompt` (Prompt) → appends all sections from the given `Prompt`
- `import` (path: string) → appends `Imported` node with format inferred from extension, wrapped in code block
- `importRaw` (path: string) → appends `Imported` node without code block wrapping
- `template` (text: string) → appends `Template` node
- `templateFile` (path: string) → appends `TemplateFile` node

#### Scenario: Add a named section
- **WHEN** `skill { section "Overview" "This skill does X." }` is evaluated
- **THEN** `Sections` contains `Section("Overview", [Text "This skill does X."])`

#### Scenario: Compose from a prompt
- **WHEN** a `Prompt` with one section is passed via `prompt` operation
- **THEN** that section is appended to `Sections`

#### Scenario: Add a template node
- **WHEN** `skill { template "Hello {{{name}}}" }` is evaluated
- **THEN** `Sections` contains `Template "Hello {{{name}}}"`

### Requirement: Library-level exposure
The system SHALL expose `Skill` as a top-level type alias and `skill` as a top-level DSL value in `FsAgent` / `FsAgent.DSL`, alongside the existing `Agent`, `Prompt`, and `SlashCommand` exports.

#### Scenario: Open FsAgent and use skill builder
- **WHEN** a consumer opens `FsAgent` and writes `let s = DSL.skill { name "x" }`
- **THEN** the code compiles and `s` is of type `Skill`
