# Spec: OpenCode Skill Path Option

## Purpose

Provides a `FolderVariant` discriminated union and overloaded path resolution functions that let callers choose which tool-specific folder (`.agents/`, `.opencode/`, or `.claude/`) is used for OpenCode project-scope skill artifacts. The default behaviour targets the cross-tool `.agents/skills/` path.

## Requirements

### Requirement: Resolve OpenCode project-scope skill to .agents/skills/ by default
The system SHALL resolve `SkillArtifact` for `AgentHarness.Opencode` + `Project` scope to the cross-tool Agent Skills path `.agents/skills/<name>/SKILL.md` when no explicit `FolderVariant` is provided.

#### Scenario: Default overload uses AgentsFolder
- **WHEN** `resolveOutputPath Opencode SkillArtifact "my-skill" (Project "/repo")` is called (arity-4 overload)
- **THEN** the returned path ends with `.agents/skills/my-skill/SKILL.md`

---

### Requirement: Support explicit FolderVariant option
The system SHALL provide a `FolderVariant` DU with cases `AgentsFolder`, `OpencodeFolder`, and `ClaudeFolder`, and an arity-5 overload of `resolveOutputPath` accepting this option.

#### Scenario: AgentsFolder explicit
- **WHEN** `resolveOutputPathWith Opencode SkillArtifact "my-skill" (Project "/repo") AgentsFolder` is called
- **THEN** the returned path ends with `.agents/skills/my-skill/SKILL.md`

#### Scenario: OpencodeFolder explicit
- **WHEN** `resolveOutputPathWith Opencode SkillArtifact "my-skill" (Project "/repo") OpencodeFolder` is called
- **THEN** the returned path ends with `.opencode/skills/my-skill/SKILL.md`

#### Scenario: ClaudeFolder for OpenCode
- **WHEN** `resolveOutputPathWith Opencode SkillArtifact "my-skill" (Project "/repo") ClaudeFolder` is called
- **THEN** the returned path ends with `.claude/skills/my-skill/SKILL.md`

#### Scenario: ClaudeFolder for Copilot
- **WHEN** `resolveOutputPathWith Copilot SkillArtifact "my-skill" (Project "/repo") ClaudeFolder` is called
- **THEN** the returned path ends with `.claude/skills/my-skill/SKILL.md`

---

### Requirement: AgentsFolder and OpencodeFolder do not affect Copilot or ClaudeCode harnesses
The `AgentsFolder` and `OpencodeFolder` cases SHALL be ignored for `Copilot` and `ClaudeCode` harnesses; their skill paths are unaffected.

#### Scenario: Copilot skill path unchanged with AgentsFolder
- **WHEN** `resolveOutputPathWith Copilot SkillArtifact "s" (Project "/r") AgentsFolder` is called
- **THEN** the returned path ends with `.github/skills/s/SKILL.md`

#### Scenario: Copilot skill path unchanged with OpencodeFolder
- **WHEN** `resolveOutputPathWith Copilot SkillArtifact "s" (Project "/r") OpencodeFolder` is called
- **THEN** the returned path ends with `.github/skills/s/SKILL.md`

#### Scenario: ClaudeCode skill path unchanged with AgentsFolder
- **WHEN** `resolveOutputPathWith ClaudeCode SkillArtifact "s" (Project "/r") AgentsFolder` is called
- **THEN** the returned path ends with `.claude/skills/s/SKILL.md`

#### Scenario: ClaudeCode skill path unchanged with OpencodeFolder
- **WHEN** `resolveOutputPathWith ClaudeCode SkillArtifact "s" (Project "/r") OpencodeFolder` is called
- **THEN** the returned path ends with `.claude/skills/s/SKILL.md`

---

### Requirement: writeSkill uses AgentsFolder default for OpenCode
The `writeSkill` module function SHALL default to `AgentsFolder` when writing OpenCode project-scope skills.

#### Scenario: writeSkill for OpenCode writes to .agents/skills/
- **WHEN** `writeSkill` is called with a named Skill, `AgentHarness.Opencode`, and `Project "<root>"`
- **THEN** the file is created at `<root>/.agents/skills/<name>/SKILL.md`
