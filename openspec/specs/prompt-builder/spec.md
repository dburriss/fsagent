## ADDED Requirements

### Requirement: Prompt type structure
The system SHALL provide a Prompt record type with Frontmatter (Map<string, obj>) and Sections (Node list) fields, matching the Agent type structure.

#### Scenario: Create empty prompt
- **WHEN** user creates a new Prompt value
- **THEN** system SHALL initialize with empty Frontmatter map and empty Sections list

### Requirement: PromptBuilder computation expression
The system SHALL provide a PromptBuilder computation expression in the FsAgent.Prompts namespace with AutoOpen attribute for immediate availability.

#### Scenario: Build prompt with builder
- **WHEN** user writes `prompt { ... }` after opening FsAgent.Prompts
- **THEN** system SHALL create a Prompt instance using the computation expression

### Requirement: Metadata operations
The system SHALL support metadata operations: name, description, author, version, license that add key-value pairs to Prompt.Frontmatter.

#### Scenario: Set prompt name
- **WHEN** user calls `name "my-prompt"` in prompt builder
- **THEN** system SHALL add "name" key with value "my-prompt" to Frontmatter map

#### Scenario: Set multiple metadata fields
- **WHEN** user sets name, description, author, version, and license
- **THEN** system SHALL store all five values in Frontmatter map with correct keys

### Requirement: Role operation
The system SHALL support `role` operation that appends a Section node with name "role" and text content to Sections list.

#### Scenario: Add role to prompt
- **WHEN** user calls `role "You are a helpful assistant"`
- **THEN** system SHALL append Section("role", [Text "You are a helpful assistant"]) to Sections

### Requirement: Objective operation
The system SHALL support `objective` operation that appends a Section node with name "objective" and text content to Sections list.

#### Scenario: Add objective to prompt
- **WHEN** user calls `objective "Help users with coding"`
- **THEN** system SHALL append Section("objective", [Text "Help users with coding"]) to Sections

### Requirement: Instructions operation
The system SHALL support `instructions` operation that appends a Section node with name "instructions" and text content to Sections list.

#### Scenario: Add instructions to prompt
- **WHEN** user calls `instructions "Be concise and accurate"`
- **THEN** system SHALL append Section("instructions", [Text "Be concise and accurate"]) to Sections

### Requirement: Context operation
The system SHALL support `context` operation that appends a Section node with name "context" and text content to Sections list.

#### Scenario: Add context to prompt
- **WHEN** user calls `context "User is a beginner programmer"`
- **THEN** system SHALL append Section("context", [Text "User is a beginner programmer"]) to Sections

### Requirement: Output operation
The system SHALL support `output` operation that appends a Section node with name "output" and text content to Sections list.

#### Scenario: Add output format to prompt
- **WHEN** user calls `output "Provide code examples in markdown"`
- **THEN** system SHALL append Section("output", [Text "Provide code examples in markdown"]) to Sections

### Requirement: Examples operation
The system SHALL support `examples` operation that takes a list of example nodes and wraps them in a Section node with name "examples".

#### Scenario: Add multiple examples to prompt
- **WHEN** user calls `examples [Prompt.example "Ex1" "Content1"; Prompt.example "Ex2" "Content2"]`
- **THEN** system SHALL append Section("examples", [list of example sections]) to Sections

### Requirement: Generic section operation
The system SHALL support `section` operation that takes a name and content string and creates a custom Section node.

#### Scenario: Add custom section to prompt
- **WHEN** user calls `section "prerequisites" "Python 3.8 or higher"`
- **THEN** system SHALL append Section("prerequisites", [Text "Python 3.8 or higher"]) to Sections

### Requirement: Import operation
The system SHALL support `import` operation that appends an Imported node with code block wrapping enabled.

#### Scenario: Import data file in prompt
- **WHEN** user calls `import "data.json"`
- **THEN** system SHALL append Imported("data.json", Json, true) to Sections

### Requirement: ImportRaw operation
The system SHALL support `importRaw` operation that appends an Imported node with code block wrapping disabled.

#### Scenario: Import raw file in prompt
- **WHEN** user calls `importRaw "examples.md"`
- **THEN** system SHALL append Imported("examples.md", Unknown, false) to Sections

### Requirement: Template operation
The system SHALL support `template` operation that appends a Template node with inline text to Sections list.

#### Scenario: Add inline template to prompt
- **WHEN** user calls `template "Hello {{{name}}}"`
- **THEN** system SHALL append Template("Hello {{{name}}}") to Sections

### Requirement: TemplateFile operation
The system SHALL support `templateFile` operation that appends a TemplateFile node with file path to Sections list.

#### Scenario: Add template file reference to prompt
- **WHEN** user calls `templateFile "templates/greeting.txt"`
- **THEN** system SHALL append TemplateFile("templates/greeting.txt") to Sections

### Requirement: Operation ordering
The system SHALL append operations to Sections in the order they are called in the builder.

#### Scenario: Multiple operations preserve order
- **WHEN** user calls `role "..." then objective "..." then template "..."`
- **THEN** system SHALL create Sections list with role Section first, objective Section second, Template node third

### Requirement: Prompt module constructors
The system SHALL provide Prompt module with constructor functions: role, objective, instructions, context, output, example, examples that return Node values.

#### Scenario: Use Prompt.role function directly
- **WHEN** user calls `Prompt.role "You are helpful"`
- **THEN** system SHALL return Section("role", [Text "You are helpful"])

#### Scenario: Use example constructor
- **WHEN** user calls `Prompt.example "Title" "Content"`
- **THEN** system SHALL return Section("example", [Text "Title"; Text "Content"])
