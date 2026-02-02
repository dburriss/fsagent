## ADDED Requirements

### Requirement: Tool discriminated union with complete tool coverage
The system SHALL define a Tool discriminated union in FsAgent.AST namespace with all tool variants for agent harnesses.

#### Scenario: Tool type structure
- **WHEN** the Tool type is defined
- **THEN** it SHALL include the following variants: Write, Edit, Shell, Read, Glob, List, LSP, Skill, TodoWrite, TodoRead, WebFetch, WebSearch, Question, Bash (legacy), Todo (legacy), and Custom of string

### Requirement: Read tool variant
The system SHALL provide Read tool variant for file reading capability.

#### Scenario: Read tool is available
- **WHEN** Tool.Read is referenced in code
- **THEN** it SHALL compile successfully and be a valid Tool variant

### Requirement: Glob tool variant
The system SHALL provide Glob tool variant for file pattern matching capability.

#### Scenario: Glob tool is available
- **WHEN** Tool.Glob is referenced in code
- **THEN** it SHALL compile successfully and be a valid Tool variant

### Requirement: List tool variant
The system SHALL provide List tool variant for directory listing capability.

#### Scenario: List tool is available
- **WHEN** Tool.List is referenced in code
- **THEN** it SHALL compile successfully and be a valid Tool variant

### Requirement: LSP tool variant
The system SHALL provide LSP tool variant for Language Server Protocol capability.

#### Scenario: LSP tool is available
- **WHEN** Tool.LSP is referenced in code
- **THEN** it SHALL compile successfully and be a valid Tool variant

### Requirement: Skill tool variant
The system SHALL provide Skill tool variant for executing predefined skills.

#### Scenario: Skill tool is available
- **WHEN** Tool.Skill is referenced in code
- **THEN** it SHALL compile successfully and be a valid Tool variant

### Requirement: TodoWrite tool variant
The system SHALL provide TodoWrite tool variant for task management write operations.

#### Scenario: TodoWrite tool is available
- **WHEN** Tool.TodoWrite is referenced in code
- **THEN** it SHALL compile successfully and be a valid Tool variant

### Requirement: TodoRead tool variant
The system SHALL provide TodoRead tool variant for task management read operations.

#### Scenario: TodoRead tool is available
- **WHEN** Tool.TodoRead is referenced in code
- **THEN** it SHALL compile successfully and be a valid Tool variant

### Requirement: WebSearch tool variant
The system SHALL provide WebSearch tool variant for web search capability.

#### Scenario: WebSearch tool is available
- **WHEN** Tool.WebSearch is referenced in code
- **THEN** it SHALL compile successfully and be a valid Tool variant

### Requirement: Question tool variant
The system SHALL provide Question tool variant for asking user questions.

#### Scenario: Question tool is available
- **WHEN** Tool.Question is referenced in code
- **THEN** it SHALL compile successfully and be a valid Tool variant

### Requirement: Shell tool variant
The system SHALL provide Shell tool variant as the primary name for shell command execution capability.

#### Scenario: Shell tool is available
- **WHEN** Tool.Shell is referenced in code
- **THEN** it SHALL compile successfully and be a valid Tool variant

### Requirement: Legacy Bash tool variant for backward compatibility
The system SHALL maintain Bash tool variant as a legacy alias for Shell to preserve backward compatibility.

#### Scenario: Bash tool remains available
- **WHEN** Tool.Bash is referenced in existing code
- **THEN** it SHALL compile successfully and behave identically to Tool.Shell

### Requirement: Legacy Todo tool variant for backward compatibility
The system SHALL maintain Todo tool variant as a legacy alias that encompasses both TodoWrite and TodoRead capabilities.

#### Scenario: Todo tool remains available
- **WHEN** Tool.Todo is referenced in existing code
- **THEN** it SHALL compile successfully and map to both TodoWrite and TodoRead harness tools

### Requirement: Tool type documentation
The system SHALL provide XML documentation for each Tool variant describing its purpose and harness mapping behavior.

#### Scenario: Write tool documentation
- **WHEN** Tool.Write documentation is viewed
- **THEN** it SHALL describe it as "File writing capability" with harness-specific name mappings

#### Scenario: Shell tool documentation
- **WHEN** Tool.Shell documentation is viewed
- **THEN** it SHALL describe it as "Shell command execution capability" with harness-specific name mappings

#### Scenario: Custom tool documentation
- **WHEN** Tool.Custom documentation is viewed
- **THEN** it SHALL explain that custom tool names pass through unchanged to all harnesses
