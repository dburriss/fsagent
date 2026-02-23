# Spec: Agent File Writer

## Purpose

Provides an `AgentFileWriter` class with injectable `IFileSystem` (from `Testably.Abstractions`) for testable, harness-aware file writing of agents, skills, and commands. Enables in-memory testing without real disk access.

## Requirements

### Requirement: AgentFileWriter accepts IFileSystem for injectable I/O
The system SHALL provide an `AgentFileWriter` class in the `FsAgent.Writers` namespace that accepts `IFileSystem` (from `Testably.Abstractions`) as a constructor parameter, enabling in-memory testing without real disk access.

Constructor signature:
```fsharp
type AgentFileWriter(
    fileSystem    : IFileSystem,
    scope         : WriteScope,
    ?configure    : AgentWriter.Options -> unit,
    ?copilotRoot  : string,
    ?folderVariant: FolderVariant)
```

#### Scenario: AgentFileWriter with MockFileSystem does not touch disk
- **WHEN** `AgentFileWriter` is constructed with a `MockFileSystem` and `WriteAgent` is called
- **THEN** the file exists in the mock filesystem
- **THEN** no real filesystem I/O occurs

---

### Requirement: AgentFileWriter.WriteAgent writes agent to correct path
The system SHALL provide `WriteAgent(agent: Agent, harness: AgentHarness) -> string` that renders the agent and writes it to the harness-correct path via the injected `IFileSystem`.

#### Scenario: WriteAgent returns resolved path
- **WHEN** `WriteAgent(myAgent, AgentHarness.Opencode)` is called with scope `Project "/repo"`
- **THEN** the returned path equals `resolveOutputPath Opencode AgentArtifact "<name>" (Project "/repo")`
- **THEN** the file exists at that path in the filesystem

---

### Requirement: AgentFileWriter.WriteSkill writes skill to correct path
The system SHALL provide `WriteSkill(skill: Skill, harness: AgentHarness) -> string` that renders the skill and writes it using the injected filesystem. The `?folderVariant` constructor parameter controls the skill root for OpenCode and (when `ClaudeFolder`) Copilot.

#### Scenario: WriteSkill for ClaudeCode uses .claude/skills/
- **WHEN** `WriteSkill(mySkill, AgentHarness.ClaudeCode)` is called with scope `Project "/repo"`
- **THEN** the file exists at `/repo/.claude/skills/<name>/SKILL.md`

#### Scenario: WriteSkill for OpenCode defaults to .agents/skills/
- **WHEN** `WriteSkill(mySkill, AgentHarness.Opencode)` is called with no `?folderVariant` and scope `Project "/repo"`
- **THEN** the file exists at `/repo/.agents/skills/<name>/SKILL.md`

#### Scenario: WriteSkill for OpenCode with ClaudeFolder uses .claude/skills/
- **WHEN** `AgentFileWriter` is constructed with `folderVariant = ClaudeFolder` and `WriteSkill(mySkill, AgentHarness.Opencode)` is called with scope `Project "/repo"`
- **THEN** the file exists at `/repo/.claude/skills/<name>/SKILL.md`

#### Scenario: WriteSkill for Copilot with ClaudeFolder uses .claude/skills/
- **WHEN** `AgentFileWriter` is constructed with `folderVariant = ClaudeFolder` and `WriteSkill(mySkill, AgentHarness.Copilot)` is called with scope `Project "/repo"`
- **THEN** the file exists at `/repo/.claude/skills/<name>/SKILL.md`

---

### Requirement: AgentFileWriter.WriteCommand writes command to correct path
The system SHALL provide `WriteCommand(cmd: SlashCommand, harness: AgentHarness, ?ns: string) -> string` that renders the command and writes it using the injected filesystem.

#### Scenario: WriteCommand for Copilot uses .prompt.md
- **WHEN** `WriteCommand(myCmd, AgentHarness.Copilot)` is called with scope `Project "/repo"`
- **THEN** the file exists at `/repo/.github/prompts/<name>.prompt.md`

#### Scenario: WriteCommand for ClaudeCode with namespace
- **WHEN** `WriteCommand(myCmd, AgentHarness.ClaudeCode, ns = "opsx")` is called with scope `Project "/repo"`
- **THEN** the file exists at `/repo/.claude/commands/opsx/<name>.md`

---

### Requirement: AgentFileWriter is accessible via open FsAgent
The `AgentFileWriter` type SHALL be re-exported from `Library.fs` so consumers using `open FsAgent` can instantiate it without opening `FsAgent.Writers`.

#### Scenario: AgentFileWriter available after open FsAgent
- **WHEN** a consumer writes `open FsAgent` and constructs `AgentFileWriter(fs, Project ".")`
- **THEN** the code compiles
