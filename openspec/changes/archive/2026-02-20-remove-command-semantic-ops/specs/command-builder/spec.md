## ADDED Requirements

### Requirement: SlashCommand type structure
The system SHALL provide a SlashCommand record type with Name (string), Description (string), and Sections (Node list) fields.

#### Scenario: Create empty slash command
- **WHEN** user creates a new SlashCommand value
- **THEN** system SHALL initialize with empty Name, empty Description, and empty Sections list

### Requirement: CommandBuilder computation expression
The system SHALL provide a CommandBuilder computation expression in the FsAgent.Commands namespace with AutoOpen attribute, accessible via the `command` binding.

#### Scenario: Build command with builder
- **WHEN** user writes `command { ... }` after opening FsAgent.Commands
- **THEN** system SHALL create a SlashCommand instance using the computation expression

### Requirement: Name operation
The system SHALL support a `name` operation that sets the Name field on the SlashCommand.

#### Scenario: Set command name
- **WHEN** user calls `name "my-command"` in command builder
- **THEN** system SHALL set SlashCommand.Name to "my-command"

### Requirement: Description operation
The system SHALL support a `description` operation that sets the Description field on the SlashCommand.

#### Scenario: Set command description
- **WHEN** user calls `description "Does something useful"` in command builder
- **THEN** system SHALL set SlashCommand.Description to "Does something useful"

### Requirement: Section operation
The system SHALL support a `section` operation that takes a name and content string and appends a custom Section node to Sections.

#### Scenario: Add custom section to command
- **WHEN** user calls `section "notes" "Extra context here"` in command builder
- **THEN** system SHALL append Section("notes", [Text "Extra context here"]) to Sections

### Requirement: Import operation
The system SHALL support an `import` operation that appends an Imported node with code block wrapping enabled to Sections.

#### Scenario: Import file into command
- **WHEN** user calls `import "data.json"` in command builder
- **THEN** system SHALL append Imported("data.json", Json, true) to Sections

### Requirement: ImportRaw operation
The system SHALL support an `importRaw` operation that appends an Imported node with code block wrapping disabled to Sections.

#### Scenario: Import raw file into command
- **WHEN** user calls `importRaw "examples.md"` in command builder
- **THEN** system SHALL append Imported("examples.md", Unknown, false) to Sections

### Requirement: Template operation
The system SHALL support a `template` operation that appends a Template node with inline text to Sections.

#### Scenario: Add inline template to command
- **WHEN** user calls `template "Hello {{{name}}}"` in command builder
- **THEN** system SHALL append Template("Hello {{{name}}}") to Sections

### Requirement: TemplateFile operation
The system SHALL support a `templateFile` operation that appends a TemplateFile node with a file path to Sections.

#### Scenario: Add template file reference to command
- **WHEN** user calls `templateFile "templates/cmd.txt"` in command builder
- **THEN** system SHALL append TemplateFile("templates/cmd.txt") to Sections

### Requirement: Prompt composition operation
The system SHALL support a `prompt` operation that takes a Prompt value and inlines its Sections into the command's Sections list.

#### Scenario: Compose prompt into command
- **WHEN** user calls `prompt (prompt { role "You are a helper"; instructions "Be concise" })` in command builder
- **THEN** system SHALL append all sections from the Prompt value to SlashCommand.Sections in order

### Requirement: Semantic content ops are NOT supported directly
The system SHALL NOT provide `role`, `objective`, `instructions`, `context`, `output`, or `examples` as direct operations on CommandBuilder. Callers MUST use `prompt { ... }` composition to include semantic content.

#### Scenario: Content requires prompt composition
- **WHEN** user needs to add role or instructions to a command
- **THEN** user SHALL wrap content in `prompt { ... }` and pass it via the `prompt` operation on CommandBuilder
