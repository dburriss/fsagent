## Why

Users of FsAgent need to generate slash command files (`.opencode/commands/*.md`, `.github/copilot-instructions/*.md`) from F# code, just as they generate agents and prompts. Today there is no DSL type for slash commands, forcing users to hand-author these files or repurpose the agent builder in ways that don't match the format.

## What Changes

- Add `SlashCommand` record type (`Name`, `Description`, `Sections`) in a new `Command.fs` module.
- Add `command { }` computation expression builder with operations mirroring the prompt builder (`name`, `description`, `role`, `objective`, `instructions`, `context`, `output`, `section`, `import`, `importRaw`, `template`, `templateFile`, `examples`, `prompt`).
- Add `AgentWriter.renderCommand` function that renders a `SlashCommand` to Markdown with `description`-only frontmatter.
- Re-export `SlashCommand` type alias and `command` CE from `Library.fs` for top-level access.
- Add acceptance tests covering frontmatter rendering, section rendering, template resolution, and harness parity.

No breaking changes. No modifications to `Agent`, `Prompt`, `AST`, or existing writer logic.

## Capabilities

### New Capabilities
- `slash-command-builder`: DSL computation expression and `SlashCommand` type for building slash command definitions in F#.
- `slash-command-writer`: Writer function that renders a `SlashCommand` to Markdown with `description` frontmatter, reusing existing section/template/import rendering.

### Modified Capabilities
<!-- No existing specs require requirement-level changes. -->

## Impact

- **New file**: `src/FsAgent/Command.fs`
- **Modified files**: `src/FsAgent/FsAgent.fsproj`, `src/FsAgent/Writers.fs`, `src/FsAgent/Library.fs`
- **New test file**: `tests/FsAgent.Tests/CommandTests.fs`
- **Modified test project**: `tests/FsAgent.Tests/FsAgent.Tests.fsproj`
- **Changelog**: `CHANGELOG.md`
- No dependency changes. No public API removals.
