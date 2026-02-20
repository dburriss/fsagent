## Why

`CommandBuilder` duplicates the semantic content operations (`role`, `objective`, `instructions`, `context`, `output`, `examples`) that already belong to `PromptBuilder`, creating two competing surfaces for the same concepts. Removing them enforces a clear separation: `CommandBuilder` composes structure, `PromptBuilder` defines content.

## What Changes

- **BREAKING**: Remove `role`, `objective`, `instructions`, `context`, `output`, `examples` custom operations from `CommandBuilder`
- Keep `name`, `description` (metadata), `section`, `import`, `importRaw`, `template`, `templateFile` (structural), and `prompt` (composition) in `CommandBuilder`
- Migrate `CommandTests.fs` usages of the removed ops to `prompt { ... }` composition
- Document the breaking change in `CHANGELOG.md`

## Capabilities

### New Capabilities

- `command-builder`: Documents the intended public surface of `CommandBuilder` — structural/metadata operations only, with `prompt` as the composition hook for content.

### Modified Capabilities

_None — no existing spec tracks `CommandBuilder` requirements._

## Impact

- `src/FsAgent/Command.fs`: Remove 6 custom operations from `CommandBuilder`
- `tests/FsAgent.Tests/CommandTests.fs`: Migrate direct semantic ops to `prompt { ... }` composition
- `CHANGELOG.md`: Breaking change entry under `[Unreleased]`
- Public DSL: Any caller using `command { role ... }`, `command { instructions ... }`, etc. must wrap content in `prompt { ... }`
