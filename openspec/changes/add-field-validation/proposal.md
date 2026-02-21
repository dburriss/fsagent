## Why

`renderCommand` and `renderSkill` currently have no validation, so missing or blank required fields produce malformed output silently. Adding collect-then-throw validation at writer time gives users a clear, complete error message consistent with the existing `renderAgent` Copilot guard.

## What Changes

- `renderCommand` validates that `Name` and `Description` are non-empty/non-whitespace before serializing; throws a single exception listing all violations.
- `renderSkill` validates that `name` and `description` entries in `Frontmatter` are present and non-empty/non-whitespace before serializing; throws a single exception listing all violations.
- `renderAgent` (Copilot path) replaces its sequential fail-fast guards with the same collect-then-throw pattern for consistency.
- New tests assert that both field names appear in the error message when both are missing.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `slash-command-writer`: Writer now validates required fields (`name`, `description`) at render time, reporting all violations in a single exception.
- `skill-writer`: Writer now validates required frontmatter fields (`name`, `description`) at render time, reporting all violations in a single exception.

## Impact

- `src/FsAgent/Writers.fs` — validation logic added/updated in `renderAgent`, `renderCommand`, `renderSkill`
- `tests/FsAgent.Tests/CommandTests.fs` — one new C test
- `tests/FsAgent.Tests/SkillTests.fs` — one new C test
- `CHANGELOG.md` — entry under `[Unreleased]`
- No DSL, AST, or builder changes; no breaking changes to valid call sites
