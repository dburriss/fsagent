## REMOVED Requirements

### Requirement: Core section operations
**Reason**: Prompt content operations (role, objective, instructions, context, output) are being moved to PromptBuilder to enable prompt reusability and separation of concerns.
**Migration**: Create prompts using `prompt { role "..."; objective "..." }` and reference them in agents via `prompt` operation.

### Requirement: Examples section
**Reason**: Examples are part of prompt content and should be defined in Prompt type, not Agent builder.
**Migration**: Add examples to prompts using `prompt { examples [...] }` and reference the prompt in the agent.

## ADDED Requirements

### Requirement: Prompt reference operation
The system SHALL support `prompt` operation in AgentBuilder that accepts a Prompt instance and merges its sections into agent sections.

#### Scenario: Reference prompt in agent
- **WHEN** user calls `prompt myPrompt` where myPrompt is a Prompt instance
- **THEN** system SHALL append all sections from myPrompt.Sections to agent.Sections

#### Scenario: Multiple prompts in agent
- **WHEN** user calls `prompt prompt1` then `prompt prompt2`
- **THEN** system SHALL append prompt1 sections followed by prompt2 sections to agent.Sections

### Requirement: Agent-specific metadata operations
The system SHALL support metadata operations specific to agent configuration: name, description, author, version, license, model, temperature, maxTokens.

#### Scenario: Set model in agent
- **WHEN** user calls `model "gpt-4"` in agent builder
- **THEN** system SHALL add "model" key with value "gpt-4" to Frontmatter map

#### Scenario: Set temperature in agent
- **WHEN** user calls `temperature 0.7` in agent builder
- **THEN** system SHALL add "temperature" key with value 0.7 to Frontmatter map

#### Scenario: Set maxTokens in agent
- **WHEN** user calls `maxTokens 2000` in agent builder
- **THEN** system SHALL add "maxTokens" key with value 2000 to Frontmatter map

### Requirement: Tool configuration operations
The system SHALL support three operations for configuring tools: `tools` (enable list), `toolMap` (explicit enable/disable), and `disallowedTools` (disable specific tools). All tool operations SHALL store tools internally as `Map<string, obj>` with boolean values.

#### Scenario: Enable tools with list operation
- **WHEN** user calls `tools ["tool1" :> obj; "tool2" :> obj]` in agent builder
- **THEN** system SHALL add "tools" key to Frontmatter map as `Map<string, obj>` with values {tool1: true, tool2: true}
- **AND** internal storage is always map format, not list

#### Scenario: Configure tools with explicit enable/disable
- **WHEN** user calls `toolMap [("bash", false); ("edit", true); ("read", true)]` in agent builder
- **THEN** system SHALL add "tools" key to Frontmatter map as `Map<string, obj>` with values {bash: false, edit: true, read: true}

#### Scenario: Disable specific tools
- **WHEN** user calls `disallowedTools ["bash"; "write"]` in agent builder
- **THEN** system SHALL merge into existing tools map, setting bash and write to false
- **AND** if no existing tools map exists, create new map with only disabled tools

#### Scenario: Combine tools and disallowedTools
- **WHEN** user calls `tools ["grep" :> obj; "bash" :> obj]` followed by `disallowedTools ["bash"]`
- **THEN** resulting map SHALL be {grep: true, bash: false}
- **AND** disallowedTools overrides previous enable values

### Requirement: Namespace relocation
The system SHALL provide AgentBuilder in FsAgent.Agents namespace with AutoOpen attribute instead of FsAgent.DSL module.

#### Scenario: Access agent builder from Agents namespace
- **WHEN** user opens FsAgent.Agents
- **THEN** system SHALL make agent and meta builders available via AutoOpen

### Requirement: Generic section and import operations remain
The system SHALL continue to support `section`, `import`, and `importRaw` operations in AgentBuilder for agent-specific content.

#### Scenario: Add custom section to agent
- **WHEN** user calls `section "deployment" "Production environment"`
- **THEN** system SHALL append Section("deployment", [Text "Production environment"]) to agent Sections

#### Scenario: Import data in agent
- **WHEN** user calls `import "config.json"` in agent builder
- **THEN** system SHALL append Imported("config.json", Json, true) to agent Sections
