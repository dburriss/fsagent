## MODIFIED Requirements

### Requirement: renderPrompt function for Prompt type
The system SHALL provide renderPrompt function that accepts Prompt and Options configuration function.

#### Scenario: Function signature
- **WHEN** renderPrompt is called
- **THEN** signature SHALL be (Prompt -> (Options -> unit) -> string)

### Requirement: No frontmatter output for prompts
The system SHALL NOT output YAML frontmatter block when writing prompts, even though Prompt.Frontmatter may contain data.

#### Scenario: Prompt with metadata writes no frontmatter
- **WHEN** Prompt.Frontmatter contains "name" and "description" keys AND renderPrompt is called with OutputType Md
- **THEN** output SHALL NOT include "---" YAML frontmatter block

### Requirement: Namespace relocation for AgentWriter
The system SHALL provide AgentWriter module in FsAgent.Writers namespace instead of FsAgent module.

#### Scenario: Access writer from Writers namespace
- **WHEN** user opens FsAgent.Writers
- **THEN** AgentWriter module SHALL be available

## REMOVED Requirements

### Requirement: writeMarkdown alias
**Reason**: Redundant alias for renderAgent (formerly writeAgent). Removed as part of pre-1.0 clean rename to avoid carrying a misleading duplicate into the stable API.
**Migration**: Replace all calls to `MarkdownWriter.writeMarkdown` with `AgentWriter.renderAgent`.
