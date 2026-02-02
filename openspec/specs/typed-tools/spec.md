## ADDED Requirements

### Requirement: Tool discriminated union

The library SHALL provide a `Tool` discriminated union type for type-safe tool references.

#### Scenario: Universal tool cases
- **WHEN** the Tool type is defined
- **THEN** it SHALL have cases for universal tools: Write, Edit, Bash, WebFetch, and Todo

#### Scenario: Custom tool escape hatch
- **WHEN** a tool name is not in the universal set
- **THEN** the Tool type SHALL provide a Custom case that accepts a string parameter

#### Scenario: Tool type location
- **WHEN** defining the Tool type
- **THEN** it SHALL be placed in AST.fs alongside other fundamental types like DataFormat

### Requirement: Tool type public API

The Tool type SHALL be accessible from the public API without requiring additional namespace imports.

#### Scenario: Re-export from Library module
- **WHEN** a library consumer imports FsAgent
- **THEN** the Tool type SHALL be available directly

#### Scenario: IDE discoverability
- **WHEN** typing "Tool." in an IDE
- **THEN** autocomplete SHALL show all available tool cases (Write, Edit, Bash, WebFetch, Todo, Custom)

### Requirement: tools operation accepts Tool list

The agent builder `tools` operation SHALL accept a `Tool list` parameter instead of `obj list`.

#### Scenario: Type-safe tool list
- **WHEN** calling the tools operation
- **THEN** it SHALL only accept Tool discriminated union values

#### Scenario: Compile-time checking
- **WHEN** attempting to pass an invalid tool value
- **THEN** a compilation error SHALL occur

#### Scenario: Tool list storage
- **WHEN** the tools operation is called
- **THEN** the Tool list SHALL be stored in frontmatter["tools"] as a boxed object, NOT converted to strings

### Requirement: disallowedTools operation accepts Tool list

The agent builder `disallowedTools` operation SHALL accept a `Tool list` parameter instead of `string list`.

#### Scenario: Type-safe disallowed list
- **WHEN** calling the disallowedTools operation
- **THEN** it SHALL only accept Tool discriminated union values

#### Scenario: Separate storage
- **WHEN** the disallowedTools operation is called
- **THEN** the Tool list SHALL be stored in frontmatter["disallowedTools"], NOT merged with tools at definition time

### Requirement: toolMap operation removed

The agent builder SHALL NOT provide a `toolMap` operation. The operation SHALL be completely removed from the API.

#### Scenario: toolMap does not exist
- **WHEN** attempting to use toolMap in agent builder
- **THEN** a compilation error SHALL occur indicating the operation does not exist

#### Scenario: Migration path documented
- **WHEN** users need to migrate from toolMap
- **THEN** they SHALL use tools and disallowedTools operations instead

### Requirement: Harness-specific tool name mapping

The writer SHALL convert Tool values to strings based on the target AgentHarness only at serialization time.

#### Scenario: toolToString function exists
- **WHEN** the writer needs to convert a Tool to a string
- **THEN** it SHALL use a toolToString function that accepts both AgentHarness and Tool parameters

#### Scenario: Opencode tool names
- **WHEN** converting tools for Opencode harness
- **THEN** tool names SHALL match Opencode specification: "write", "edit", "bash", "webfetch", "todo"

#### Scenario: Copilot tool names
- **WHEN** converting tools for Copilot harness
- **THEN** tool names SHALL match Copilot specification (verify from documentation)

#### Scenario: ClaudeCode tool names
- **WHEN** converting tools for ClaudeCode harness
- **THEN** tool names SHALL match Claude Code specification (may be capitalized: "Write", "Edit", etc.)

#### Scenario: Custom tool passthrough
- **WHEN** converting a Custom tool for any harness
- **THEN** the string value SHALL pass through unchanged

### Requirement: formatToolsFrontmatter extracts Tool lists

The formatToolsFrontmatter function SHALL extract Tool lists from frontmatter and convert them to strings.

#### Scenario: Extract enabled tools
- **WHEN** formatting frontmatter
- **THEN** the function SHALL extract the Tool list from frontmatter["tools"]

#### Scenario: Extract disabled tools
- **WHEN** formatting frontmatter
- **THEN** the function SHALL extract the Tool list from frontmatter["disallowedTools"]

#### Scenario: Convert to string maps
- **WHEN** Tool lists are extracted
- **THEN** each Tool SHALL be converted to a string using toolToString with the current AgentHarness

#### Scenario: Merge enabled and disabled
- **WHEN** both tools and disallowedTools exist
- **THEN** they SHALL be merged into a single map where disabled tools override enabled ones

### Requirement: Delayed tool resolution

Tool values SHALL remain as Tool types until serialization time. String conversion SHALL NOT occur at agent definition time.

#### Scenario: Frontmatter stores Tool types
- **WHEN** agent definition uses tools operation
- **THEN** frontmatter SHALL contain Tool list objects, not string maps

#### Scenario: String conversion timing
- **WHEN** writeMarkdown is called
- **THEN** Tool-to-string conversion SHALL occur during serialization, not before

#### Scenario: Harness context available
- **WHEN** converting Tool to string
- **THEN** the target AgentHarness SHALL be known and used for platform-specific mapping

### Requirement: Breaking changes documented

The library SHALL clearly document all breaking changes related to typed tools.

#### Scenario: Signature changes documented
- **WHEN** users upgrade to the new version
- **THEN** breaking changes SHALL include: tools signature (obj list → Tool list), disallowedTools signature (string list → Tool list), toolMap removal, frontmatter storage format change

#### Scenario: Migration guide provided
- **WHEN** users need to migrate existing code
- **THEN** documentation SHALL provide clear examples of how to update agent definitions

### Requirement: Type safety benefits

The typed tool system SHALL provide compile-time safety and IDE support.

#### Scenario: Invalid tool names caught at compile time
- **WHEN** a developer types an incorrect tool name
- **THEN** a compilation error SHALL occur before runtime

#### Scenario: IDE autocomplete for tools
- **WHEN** typing in agent builder with tools operation
- **THEN** IDE SHALL suggest valid Tool cases (Write, Edit, Bash, WebFetch, Todo, Custom)

#### Scenario: No string typos
- **WHEN** using typed tools instead of strings
- **THEN** common mistakes like "bash " (trailing space) or "Write" (wrong case) SHALL be impossible
