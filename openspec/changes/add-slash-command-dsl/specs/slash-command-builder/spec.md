## ADDED Requirements

### Requirement: SlashCommand type exists
The system SHALL provide a `SlashCommand` record type with fields `Name: string`, `Description: string`, and `Sections: Node list` in the `FsAgent.Commands` namespace.

#### Scenario: SlashCommand can be constructed
- **WHEN** a `SlashCommand` value is created with name, description, and sections
- **THEN** all three fields are accessible on the resulting record

### Requirement: command CE builder produces a SlashCommand
The system SHALL provide a `command` computation expression that builds a `SlashCommand` value. It SHALL be accessible as `FsAgent.Commands.command` and re-exported from `FsAgent.DSL.command`.

#### Scenario: Empty command builds with defaults
- **WHEN** `command { }` is evaluated
- **THEN** the result is a `SlashCommand` with empty `Name`, empty `Description`, and empty `Sections`

#### Scenario: name operation sets Name field
- **WHEN** `command { name "my-cmd" }` is evaluated
- **THEN** the result has `Name = "my-cmd"`

#### Scenario: description operation sets Description field
- **WHEN** `command { description "Does a thing" }` is evaluated
- **THEN** the result has `Description = "Does a thing"`

### Requirement: command CE supports section-building operations
The `command` CE SHALL support the following operations, each appending the corresponding `Node` to `Sections`: `role`, `objective`, `instructions`, `context`, `output`, `section`, `import`, `importRaw`, `template`, `templateFile`, `examples`, `prompt`.

#### Scenario: instructions operation appends Section node
- **WHEN** `command { instructions "Step 1" }` is evaluated
- **THEN** `Sections` contains a `Section("instructions", [Text "Step 1"])` node

#### Scenario: template operation appends Template node
- **WHEN** `command { template "Use {{{tool Bash}}}" }` is evaluated
- **THEN** `Sections` contains a `Template "Use {{{tool Bash}}}"` node

#### Scenario: import operation appends Imported node
- **WHEN** `command { import "shared/ctx.md" }` is evaluated
- **THEN** `Sections` contains an `Imported` node referencing `"shared/ctx.md"`

#### Scenario: prompt operation merges Prompt sections
- **WHEN** a `Prompt` value with non-empty `Sections` is composed via `command { prompt p }` 
- **THEN** all sections from `p.Sections` are appended to the command's `Sections`

#### Scenario: multiple operations accumulate in order
- **WHEN** `command { role "R"; instructions "I"; section "notes" "N" }` is evaluated
- **THEN** `Sections` contains three nodes in declaration order

### Requirement: SlashCommand type alias available at top level
The system SHALL expose `type SlashCommand = FsAgent.Commands.SlashCommand` in the `FsAgent` namespace so users need only `open FsAgent`.

#### Scenario: Type alias resolves without opening Commands namespace
- **WHEN** a file opens only `FsAgent`
- **THEN** `SlashCommand` resolves to the correct record type
