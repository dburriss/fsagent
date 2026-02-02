## ADDED Requirements

### Requirement: AgentHarness type definition

The library SHALL provide an `AgentHarness` discriminated union type that represents the target execution platform for agents.

#### Scenario: Three harness cases defined
- **WHEN** the AgentHarness type is defined
- **THEN** it SHALL have exactly three cases: Opencode, Copilot, and ClaudeCode

#### Scenario: Type is accessible from public API
- **WHEN** a library consumer imports FsAgent
- **THEN** the AgentHarness type SHALL be available without requiring additional namespace imports

### Requirement: AgentFormat removed

The library SHALL NOT expose an `AgentFormat` type. All references to `AgentFormat` MUST be replaced with `AgentHarness`.

#### Scenario: AgentFormat type does not exist
- **WHEN** attempting to reference `AgentFormat` in code
- **THEN** a compilation error SHALL occur indicating the type does not exist

#### Scenario: Backward compatibility not maintained
- **WHEN** existing code uses `AgentFormat`
- **THEN** the code SHALL fail to compile, requiring migration to `AgentHarness`

### Requirement: Writer Options use AgentHarness

The MarkdownWriter Options record SHALL use `AgentHarness` for the `OutputFormat` field.

#### Scenario: OutputFormat field type
- **WHEN** configuring writer options
- **THEN** the `OutputFormat` field SHALL accept only `AgentHarness` values (Opencode, Copilot, or ClaudeCode)

#### Scenario: Default harness is Opencode
- **WHEN** creating default writer options
- **THEN** the `OutputFormat` SHALL be set to Opencode

### Requirement: WriterContext uses AgentHarness

The WriterContext record SHALL use `AgentHarness` for the `Format` field.

#### Scenario: Format field type in context
- **WHEN** the writer creates a WriterContext
- **THEN** the `Format` field SHALL contain the AgentHarness value from Options

#### Scenario: Context passed to formatting functions
- **WHEN** formatting functions receive WriterContext
- **THEN** they SHALL have access to the AgentHarness to make platform-specific decisions

### Requirement: ClaudeCode harness support

The library SHALL treat ClaudeCode as a first-class harness equal to Opencode and Copilot.

#### Scenario: ClaudeCode in pattern matching
- **WHEN** code pattern matches on AgentHarness
- **THEN** ClaudeCode SHALL be a valid case alongside Opencode and Copilot

#### Scenario: ClaudeCode agent generation
- **WHEN** writeMarkdown is called with OutputFormat set to ClaudeCode
- **THEN** the library SHALL generate agent configuration suitable for Claude Code platform

### Requirement: Semantic clarity in naming

The AgentHarness name SHALL clearly indicate it represents an execution platform/environment rather than a file format.

#### Scenario: Distinction from OutputType
- **WHEN** reviewing the API surface
- **THEN** AgentHarness (execution platform) SHALL be clearly distinct from OutputType (Md, Json, Yaml)

#### Scenario: Documentation clarity
- **WHEN** reading type names and signatures
- **THEN** the purpose of AgentHarness as a platform identifier SHALL be self-evident
