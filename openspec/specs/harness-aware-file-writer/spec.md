# Spec: Harness-Aware File Writer

## Purpose

Provides path resolution and file writing functionality for agent artifacts, scoped to a specific harness (Opencode, Copilot, ClaudeCode) and write scope (Project or Global). Resolves artifact output paths without performing I/O and provides convenience functions for writing agents, skills, and commands to their correct locations.

## Requirements

### Requirement: Resolve output path for agent artifacts
Given a harness, artifact kind, artifact name, and write scope, the system SHALL return the correct absolute file path for an agent artifact without performing any I/O.

Path conventions:
- Opencode + Project: `<rootDir>/.opencode/agents/<name>.md`
- Opencode + Global: `~/.config/opencode/agents/<name>.md` (Linux/macOS) / `%APPDATA%\opencode\agents\<name>.md` (Windows)
- Copilot + Project: `<rootDir>/.github/agents/<name>.md`
- Copilot + Global: raises `NotSupportedException`
- ClaudeCode + Project: `<rootDir>/.claude/agents/<name>.md`
- ClaudeCode + Global: `~/.claude/agents/<name>.md` (Linux/macOS) / `%USERPROFILE%\.claude\agents\<name>.md` (Windows)

#### Scenario: Resolve Opencode agent path for project scope
- **WHEN** `resolveOutputPath` is called with `AgentHarness.Opencode`, `AgentArtifact`, name `"my-bot"`, and `Project "/repo"`
- **THEN** the returned path is `/repo/.opencode/agents/my-bot.md`

#### Scenario: Resolve Copilot agent path for project scope
- **WHEN** `resolveOutputPath` is called with `AgentHarness.Copilot`, `AgentArtifact`, name `"my-bot"`, and `Project "/repo"`
- **THEN** the returned path is `/repo/.github/agents/my-bot.md`

#### Scenario: Resolve ClaudeCode agent path for project scope
- **WHEN** `resolveOutputPath` is called with `AgentHarness.ClaudeCode`, `AgentArtifact`, name `"my-bot"`, and `Project "/repo"`
- **THEN** the returned path is `/repo/.claude/agents/my-bot.md`

#### Scenario: Copilot global scope raises NotSupportedException
- **WHEN** `resolveOutputPath` is called with `AgentHarness.Copilot`, `AgentArtifact`, any name, and `Global`
- **THEN** a `NotSupportedException` is raised with a message indicating Copilot does not support global file-system scope

---

### Requirement: Resolve output path for skill artifacts
The system SHALL return the correct path for a skill artifact. Skills always use a subdirectory layout: `<root>/skills/<name>/SKILL.md`. For `AgentHarness.Opencode` + `Project` scope, the default root is `.agents/` (cross-tool Agent Skills spec path); callers MAY override via the `FolderVariant` option. `ClaudeFolder` also affects `AgentHarness.Copilot` + `Project` scope.

#### Scenario: Resolve Opencode skill path for project scope (default)
- **WHEN** `resolveOutputPath` is called with `AgentHarness.Opencode`, `SkillArtifact`, name `"my-skill"`, and `Project "/repo"` using the arity-4 default overload
- **THEN** the returned path is `/repo/.agents/skills/my-skill/SKILL.md`

#### Scenario: Resolve Opencode skill path for project scope (OpencodeFolder)
- **WHEN** `resolveOutputPathWith` is called with `AgentHarness.Opencode`, `SkillArtifact`, name `"my-skill"`, `Project "/repo"`, and `OpencodeFolder`
- **THEN** the returned path is `/repo/.opencode/skills/my-skill/SKILL.md`

#### Scenario: Resolve Opencode skill path for project scope (ClaudeFolder)
- **WHEN** `resolveOutputPathWith` is called with `AgentHarness.Opencode`, `SkillArtifact`, name `"my-skill"`, `Project "/repo"`, and `ClaudeFolder`
- **THEN** the returned path is `/repo/.claude/skills/my-skill/SKILL.md`

#### Scenario: Resolve Copilot skill path for project scope (ClaudeFolder)
- **WHEN** `resolveOutputPathWith` is called with `AgentHarness.Copilot`, `SkillArtifact`, name `"my-skill"`, `Project "/repo"`, and `ClaudeFolder`
- **THEN** the returned path is `/repo/.claude/skills/my-skill/SKILL.md`

#### Scenario: Resolve Copilot skill path for project scope
- **WHEN** `resolveOutputPath` is called with `AgentHarness.Copilot`, `SkillArtifact`, name `"my-skill"`, and `Project "/repo"`
- **THEN** the returned path is `/repo/.github/skills/my-skill/SKILL.md`

#### Scenario: Resolve ClaudeCode skill path for project scope
- **WHEN** `resolveOutputPath` is called with `AgentHarness.ClaudeCode`, `SkillArtifact`, name `"my-skill"`, and `Project "/repo"`
- **THEN** the returned path is `/repo/.claude/skills/my-skill/SKILL.md`

#### Scenario: Resolve ClaudeCode skill path for global scope
- **WHEN** `resolveOutputPath` is called with `AgentHarness.ClaudeCode`, `SkillArtifact`, name `"my-skill"`, and `Global`
- **THEN** the returned path ends with `.claude/skills/my-skill/SKILL.md`

#### Scenario: Resolve Opencode global scope skill path
- **WHEN** `resolveOutputPath` is called with `AgentHarness.Opencode`, `SkillArtifact`, name `"my-skill"`, and `Global`
- **THEN** the returned path ends with `opencode/skills/my-skill/SKILL.md`

---

### Requirement: Resolve output path for command artifacts
The system SHALL return the correct path for a command artifact. Copilot commands MUST use the `.prompt.md` double-extension. ClaudeCode commands MAY include an optional namespace subdirectory.

#### Scenario: Resolve Opencode command path for project scope
- **WHEN** `resolveOutputPath` is called with `AgentHarness.Opencode`, `CommandArtifact None`, name `"deploy"`, and `Project "/repo"`
- **THEN** the returned path is `/repo/.opencode/commands/deploy.md`

#### Scenario: Resolve Copilot command path with .prompt.md suffix
- **WHEN** `resolveOutputPath` is called with `AgentHarness.Copilot`, `CommandArtifact None`, name `"deploy"`, and `Project "/repo"`
- **THEN** the returned path is `/repo/.github/prompts/deploy.prompt.md`

#### Scenario: Resolve ClaudeCode command path without namespace
- **WHEN** `resolveOutputPath` is called with `AgentHarness.ClaudeCode`, `CommandArtifact None`, name `"deploy"`, and `Project "/repo"`
- **THEN** the returned path is `/repo/.claude/commands/deploy.md`

#### Scenario: Resolve ClaudeCode command path with namespace
- **WHEN** `resolveOutputPath` is called with `AgentHarness.ClaudeCode`, `CommandArtifact (Some "opsx")`, name `"apply"`, and `Project "/repo"`
- **THEN** the returned path is `/repo/.claude/commands/opsx/apply.md`

---

### Requirement: Resolve relative project root to absolute path
The system SHALL resolve a relative `rootDir` in `Project` scope to an absolute path before combining it with the artifact subdirectory.

#### Scenario: Relative root is normalised
- **WHEN** `resolveOutputPath` is called with `Project "."` as scope
- **THEN** the returned path is an absolute path (does not start with `.`)

---

### Requirement: Write content to the resolved path
The system SHALL write a string to the resolved output path, creating all intermediate directories as needed, and return the resolved path.

#### Scenario: File and directories are created
- **WHEN** `writeFile` is called with valid harness, kind, name, scope, and content
- **THEN** the file exists at the resolved path with the given content
- **THEN** all intermediate directories were created

#### Scenario: Existing file is overwritten
- **WHEN** `writeFile` is called and the target file already exists
- **THEN** the file content is replaced with the new content

#### Scenario: Returned path matches resolved path
- **WHEN** `writeFile` is called successfully
- **THEN** the returned string equals `resolveOutputPath` called with the same arguments

---

### Requirement: Write agent using convenience function
The system SHALL provide `writeAgent` which renders an `Agent` using `renderAgent` and writes the result to the harness-correct path, returning the path.

#### Scenario: Agent name derived from frontmatter
- **WHEN** `writeAgent` is called with an `Agent` whose frontmatter contains `name = "my-bot"`
- **THEN** the file is written at the path resolved from the name `"my-bot"`

#### Scenario: Missing agent name raises ArgumentException
- **WHEN** `writeAgent` is called with an `Agent` whose frontmatter does not contain `name`
- **THEN** an `ArgumentException` is raised

---

### Requirement: Write skill using convenience function
The system SHALL provide `writeSkill` which renders a `Skill` using `renderSkill` and writes the result to the harness-correct path, returning the path. The skill name is derived from `skill.Frontmatter["name"]`.

#### Scenario: Skill written to correct subdirectory layout
- **WHEN** `writeSkill` is called with a `Skill` named `"my-skill"` and `Project "/repo"` and `AgentHarness.Opencode`
- **THEN** the file exists at `/repo/.agents/skills/my-skill/SKILL.md`

#### Scenario: Missing skill name raises ArgumentException
- **WHEN** `writeSkill` is called with a `Skill` whose frontmatter does not contain `name`
- **THEN** an `ArgumentException` is raised

---

### Requirement: Write command using convenience function
The system SHALL provide `writeCommand` which renders a `SlashCommand` using `renderCommand` and writes the result to the harness-correct path, returning the path. The name is taken from `cmd.Name`.

#### Scenario: Copilot command written with .prompt.md extension
- **WHEN** `writeCommand` is called with `AgentHarness.Copilot`, a `SlashCommand` named `"deploy"`, and `Project "/repo"`
- **THEN** the file exists at `/repo/.github/prompts/deploy.prompt.md`

#### Scenario: ClaudeCode namespaced command written correctly
- **WHEN** `writeCommand` is called with `AgentHarness.ClaudeCode`, a `SlashCommand` named `"apply"`, namespace `Some "opsx"`, and `Project "/repo"`
- **THEN** the file exists at `/repo/.claude/commands/opsx/apply.md`
