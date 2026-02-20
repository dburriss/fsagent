## 1. Remove semantic ops from CommandBuilder

- [ ] 1.1 In `src/FsAgent/Command.fs`, remove the `role` custom operation and its implementation (lines 32–34)
- [ ] 1.2 In `src/FsAgent/Command.fs`, remove the `objective` custom operation and its implementation (lines 36–38)
- [ ] 1.3 In `src/FsAgent/Command.fs`, remove the `instructions` custom operation and its implementation (lines 40–42)
- [ ] 1.4 In `src/FsAgent/Command.fs`, remove the `context` custom operation and its implementation (lines 44–46)
- [ ] 1.5 In `src/FsAgent/Command.fs`, remove the `output` custom operation and its implementation (lines 48–50)
- [ ] 1.6 In `src/FsAgent/Command.fs`, remove the `examples` custom operation and its implementation (lines 72–74)

## 2. Migrate tests to prompt composition

- [ ] 2.1 In `tests/FsAgent.Tests/CommandTests.fs`, update test ``A: command sections render correctly`` (lines 42–53): replace `role "You are a helper"` and `instructions "Follow the steps"` with a `prompt (prompt { role "You are a helper"; instructions "Follow the steps" })` call, keeping `section "notes" "Extra notes here"` as-is
- [ ] 2.2 In `tests/FsAgent.Tests/CommandTests.fs`, update test ``A: command renders identically for Copilot and ClaudeCode`` (lines 83–94): replace `instructions "Do the thing"` with `prompt (prompt { instructions "Do the thing" })`

## 3. Verify

- [ ] 3.1 Run `dotnet build` — verify zero errors
- [ ] 3.2 Run `dotnet test` — verify all tests pass

## 4. Document

- [ ] 4.1 Add a breaking change entry to `CHANGELOG.md` under `[Unreleased]`: note that `role`, `objective`, `instructions`, `context`, `output`, and `examples` have been removed from `CommandBuilder` and callers must migrate to `prompt { ... }` composition
