## ADDED Requirements

### Requirement: Complete tool type coverage
The system SHALL support all tool variants available across the three major agent harnesses (Opencode, Claude Code, GitHub Copilot) in the Tool discriminated union.

#### Scenario: All tool variants are defined
- **WHEN** the Tool type is examined
- **THEN** it SHALL include variants for: Write, Edit, Shell, Read, Glob, List, LSP, Skill, TodoWrite, TodoRead, WebFetch, WebSearch, Question, and Custom

### Requirement: One-to-many tool mapping
The system SHALL support mapping a single FsAgent Tool variant to multiple harness-specific tool names.

#### Scenario: TodoWrite maps to multiple Claude tools
- **WHEN** TodoWrite tool is converted for ClaudeCode harness
- **THEN** it SHALL return both "TaskCreate" and "TaskUpdate"

#### Scenario: TodoRead maps to multiple Claude tools
- **WHEN** TodoRead tool is converted for ClaudeCode harness
- **THEN** it SHALL return "TaskList", "TaskGet", and "TaskUpdate"

### Requirement: Harness-specific tool name mapping
The system SHALL convert FsAgent Tool variants to the correct tool names for each target harness.

#### Scenario: Shell tool for Opencode
- **WHEN** Shell tool is converted for Opencode harness
- **THEN** it SHALL return "bash"

#### Scenario: Shell tool for ClaudeCode
- **WHEN** Shell tool is converted for ClaudeCode harness
- **THEN** it SHALL return "Bash"

#### Scenario: Shell tool for Copilot
- **WHEN** Shell tool is converted for Copilot harness
- **THEN** it SHALL return "execute"

#### Scenario: Write tool for all harnesses
- **WHEN** Write tool is converted for any harness
- **THEN** it SHALL return "write" for Opencode and Copilot, and "Write" for ClaudeCode

### Requirement: Tool deduplication
The system SHALL deduplicate harness tool names when multiple FsAgent tools map to the same underlying harness tool.

#### Scenario: Duplicate tools are removed
- **WHEN** both Glob and List tools are specified and both map to "search" for Copilot
- **THEN** the output SHALL contain only one instance of "search"

#### Scenario: Deduplication preserves first occurrence order
- **WHEN** tools [Write, Glob, List] are specified and Glob/List both map to "search"
- **THEN** the output SHALL be ["write", "search"] in that order

### Requirement: Missing tool handling
The system SHALL gracefully handle tools that are not available for specific harnesses by omitting them from the output.

#### Scenario: WebSearch omitted for Opencode
- **WHEN** WebSearch tool is specified for Opencode harness
- **THEN** it SHALL be omitted from the output (no tool added)

#### Scenario: LSP omitted for Copilot
- **WHEN** LSP tool is specified for Copilot harness
- **THEN** it SHALL be omitted from the output (no tool added)

#### Scenario: Question omitted for Copilot
- **WHEN** Question tool is specified for Copilot harness
- **THEN** it SHALL be omitted from the output (no tool added)

### Requirement: Custom tool pass-through
The system SHALL pass through Custom tool names unchanged to all harnesses.

#### Scenario: Custom tool name preserved
- **WHEN** Custom "mcp_database" is specified
- **THEN** it SHALL return "mcp_database" for all harnesses

#### Scenario: Multiple custom tools
- **WHEN** Custom "github_api" and Custom "slack_api" are specified
- **THEN** both SHALL appear in output unchanged as "github_api" and "slack_api"

### Requirement: Complete harness tool mapping matrix
The system SHALL implement the complete mapping matrix between FsAgent tools and harness-specific tool names.

#### Scenario: Read tool mapping
- **WHEN** Read tool is converted
- **THEN** it SHALL return "read" for Opencode and Copilot, and "Read" for ClaudeCode

#### Scenario: Glob tool mapping
- **WHEN** Glob tool is converted
- **THEN** it SHALL return "grep" for Opencode, "Glob" for ClaudeCode, and "search" for Copilot

#### Scenario: List tool mapping
- **WHEN** List tool is converted
- **THEN** it SHALL return "list" for Opencode, "Glob" for ClaudeCode, and "search" for Copilot

#### Scenario: LSP tool mapping
- **WHEN** LSP tool is converted
- **THEN** it SHALL return "lsp" for Opencode, "LSP" for ClaudeCode, and empty for Copilot

#### Scenario: Skill tool mapping
- **WHEN** Skill tool is converted
- **THEN** it SHALL return "skill" for all harnesses

#### Scenario: WebFetch tool mapping
- **WHEN** WebFetch tool is converted
- **THEN** it SHALL return "webfetch" for Opencode, "WebFetch" for ClaudeCode, and "web" for Copilot

#### Scenario: WebSearch tool mapping
- **WHEN** WebSearch tool is converted
- **THEN** it SHALL return empty for Opencode, "WebSearch" for ClaudeCode, and "web" for Copilot

#### Scenario: Question tool mapping
- **WHEN** Question tool is converted
- **THEN** it SHALL return "question" for Opencode, "AskUserQuestion" for ClaudeCode, and empty for Copilot

#### Scenario: Edit tool mapping
- **WHEN** Edit tool is converted
- **THEN** it SHALL return "edit" for all harnesses (with "Edit" capitalized for ClaudeCode)

### Requirement: TodoWrite tool mapping
The system SHALL map TodoWrite to appropriate write-related task tools for each harness.

#### Scenario: TodoWrite for Opencode
- **WHEN** TodoWrite tool is converted for Opencode
- **THEN** it SHALL return "todowrite"

#### Scenario: TodoWrite for Copilot
- **WHEN** TodoWrite tool is converted for Copilot
- **THEN** it SHALL return "todo"

### Requirement: TodoRead tool mapping
The system SHALL map TodoRead to appropriate read-related task tools for each harness.

#### Scenario: TodoRead for Opencode
- **WHEN** TodoRead tool is converted for Opencode
- **THEN** it SHALL return "todoread"

#### Scenario: TodoRead for Copilot
- **WHEN** TodoRead tool is converted for Copilot
- **THEN** it SHALL return "todo"
