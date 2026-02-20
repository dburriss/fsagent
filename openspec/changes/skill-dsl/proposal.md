## Why

FsAgent can generate agents, prompts, and commands but has no first-class support for skill files. Users who want to produce `SKILL.md` files for OpenCode or GitHub Copilot must assemble raw strings manually, bypassing the DSL entirely.

## What Changes

- Add a `Skill` record type with `Name`, `Description`, `License`, `Compatibility`, `Metadata`, and `Sections` fields.
- Add a `skill { ... }` computation expression (CE) in a new `FsAgent.Skills` namespace.
- Add `AgentWriter.renderSkill` to produce Markdown-only output with SKILL.md-compatible frontmatter.
- Expose `Skill` type alias and `skill` CE value in `Library.fs` for backward-compatible top-level access.
- No changes to `Agent`, `Prompt`, `Command`, or `Node`. No breaking changes.

## Capabilities

### New Capabilities
- `skill-builder`: `skill { ... }` CE and `Skill` record type for building skill definitions in the DSL layer.
- `skill-writer`: `AgentWriter.renderSkill` that renders a `Skill` to a SKILL.md-compatible Markdown string with frontmatter.

### Modified Capabilities

None.

## Impact

- **New file**: `src/FsAgent/Skill.fs` — `Skill` type + `SkillBuilder` CE
- **Modified**: `src/FsAgent/Writers.fs` — add `renderSkill` to `AgentWriter`
- **Modified**: `src/FsAgent/Library.fs` — add `Skill` alias and `skill` CE to `DSL` module
- **Modified**: `src/FsAgent/FsAgent.fsproj` — add `Skill.fs` compile entry after `Command.fs`
- **New test file**: `tests/FsAgent.Tests/SkillTests.fs`
- **Modified**: `tests/FsAgent.Tests/FsAgent.Tests.fsproj` — add `SkillTests.fs`
- No public API breakage; additive only.
