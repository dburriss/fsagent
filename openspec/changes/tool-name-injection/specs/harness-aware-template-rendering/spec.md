## ADDED Requirements

### Requirement: Tool name injection syntax in templates
The system SHALL support `{{tool <Name>}}` syntax in template strings, resolving to the harness-correct tool name string at write time.

#### Scenario: Tool name resolved for Opencode harness
- **WHEN** a template contains `{{tool Bash}}` and the harness is `Opencode`
- **THEN** the system SHALL render the tool name as the Opencode-specific name for `Bash`

#### Scenario: Tool name resolved for Copilot harness
- **WHEN** a template contains `{{tool Read}}` and the harness is `Copilot`
- **THEN** the system SHALL render the tool name as the Copilot-specific name for `Read`

#### Scenario: Tool name resolved for ClaudeCode harness
- **WHEN** a template contains `{{tool Bash}}` and the harness is `ClaudeCode`
- **THEN** the system SHALL render the tool name as the ClaudeCode-specific name for `Bash`

#### Scenario: Unknown tool name falls through to Custom
- **WHEN** a template contains `{{tool UnknownTool}}` for any harness
- **THEN** the system SHALL return the string `"UnknownTool"` unchanged

### Requirement: renderWithHarness function
The system SHALL provide a `Template.renderWithHarness` function that accepts inline template text, a `toolNameMap`, a `toolToString` resolver, an `AgentHarness`, and a `TemplateVariables` map, returning a rendered string.

#### Scenario: Render inline template with harness tool injection
- **WHEN** `Template.renderWithHarness "Use {{tool Bash}}" toolNameMap toolToString Opencode (Map [])` is called
- **THEN** the system SHALL return a string with the Opencode name for `Bash` substituted

#### Scenario: Harness-aware render preserves non-tool variables
- **WHEN** `Template.renderWithHarness "Use {{tool Bash}} and {{{var}}}" toolNameMap toolToString Opencode (Map ["var" -> "something"])` is called
- **THEN** the system SHALL resolve both `{{tool Bash}}` and `{{{var}}}` correctly in the output

### Requirement: renderFileWithHarness function
The system SHALL provide a `Template.renderFileWithHarness` function that accepts a file path, a `toolNameMap`, a `toolToString` resolver, an `AgentHarness`, and a `TemplateVariables` map, returning a rendered string.

#### Scenario: Render file template with harness tool injection
- **WHEN** file content contains `{{tool Read}}` and `Template.renderFileWithHarness path toolNameMap toolToString Copilot (Map [])` is called
- **THEN** the system SHALL return the file content with the Copilot name for `Read` substituted

#### Scenario: File not found returns error message
- **WHEN** the file path does not exist and `renderFileWithHarness` is called
- **THEN** the system SHALL return `"[Template file not found: {path}]"`

### Requirement: toolNameMap lookup by DU case name
The system SHALL build a `toolNameMap: Map<string, Tool>` inside `MarkdownWriter` that maps each `Tool` DU case name string (e.g., `"Bash"`) to its corresponding `Tool` value.

#### Scenario: Known tool case name resolves to Tool value
- **WHEN** `toolNameMap` is queried with `"Bash"`
- **THEN** the system SHALL return `Tool.Bash`

#### Scenario: Unknown tool case name resolved via Custom fallback
- **WHEN** the `tool` function in the Fue context receives a name not present in `toolNameMap`
- **THEN** the system SHALL resolve it as `Tool.Custom name`, returning the name string unchanged for all harnesses

### Requirement: Write-time harness-aware template dispatch
The system SHALL update `writeMd`'s `Template` and `TemplateFile` branches to call `renderWithHarness` and `renderFileWithHarness` respectively, passing the current harness, `toolNameMap`, and `toolToString`.

#### Scenario: Template node rendered with harness context
- **WHEN** `writeMd` processes a `Template` node and a harness is set in the format context
- **THEN** the system SHALL call `renderWithHarness` with the current harness

#### Scenario: TemplateFile node rendered with harness context
- **WHEN** `writeMd` processes a `TemplateFile` node and a harness is set in the format context
- **THEN** the system SHALL call `renderFileWithHarness` with the current harness
