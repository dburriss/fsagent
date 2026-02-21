## MODIFIED Requirements

### Requirement: renderCommand renders description-only frontmatter
The system SHALL provide `AgentWriter.renderCommand : SlashCommand -> (Options -> unit) -> string` that renders a YAML front block containing only the `description` key.

#### Scenario: description appears in frontmatter
- **WHEN** `renderCommand` is called with a command whose `Description = "Does a thing"`
- **THEN** the output begins with `---\ndescription: Does a thing\n---`

#### Scenario: name is absent from frontmatter
- **WHEN** `renderCommand` is called with a command whose `Name = "my-cmd"`
- **THEN** the output does NOT contain `name:` in the YAML front block

#### Scenario: empty description raises validation error
- **WHEN** `renderCommand` is called with `Description = ""`
- **THEN** a `System.Exception` is raised with a message containing `'description'`

#### Scenario: whitespace-only description raises validation error
- **WHEN** `renderCommand` is called with `Description = "   "`
- **THEN** a `System.Exception` is raised with a message containing `'description'`

## ADDED Requirements

### Requirement: renderCommand validates required fields before serializing
`renderCommand` SHALL validate that `Name` and `Description` are non-empty and non-whitespace before performing any output serialization. All validation errors SHALL be collected and reported in a single exception.

#### Scenario: empty name raises validation error
- **WHEN** `renderCommand` is called with `Name = ""`
- **THEN** a `System.Exception` is raised with a message containing `'name'`

#### Scenario: whitespace-only name raises validation error
- **WHEN** `renderCommand` is called with `Name = "   "`
- **THEN** a `System.Exception` is raised with a message containing `'name'`

#### Scenario: both name and description empty reports both errors
- **WHEN** `renderCommand` is called with `Name = ""` and `Description = ""`
- **THEN** a single `System.Exception` is raised whose message contains both `'name'` and `'description'`

#### Scenario: valid name and description does not raise
- **WHEN** `renderCommand` is called with non-empty `Name` and `Description`
- **THEN** no exception is raised and output is returned
