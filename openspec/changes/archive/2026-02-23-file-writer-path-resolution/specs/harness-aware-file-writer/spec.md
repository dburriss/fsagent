## MODIFIED Requirements

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
