# Plan: Remove Semantic Content Ops from CommandBuilder

Status: Draft

## Overview

`CommandBuilder` duplicates the semantic content operations (`role`, `objective`, `instructions`, `context`, `output`, `examples`) that belong to `PromptBuilder`. These will be removed from `CommandBuilder`. The structural utilities (`section`, `import`, `importRaw`, `template`, `templateFile`) and the `prompt` operation stay — they are not duplicates, they are the intended composition mechanism.

`AgentBuilder` is not changed — it only has structural utilities, not the semantic content ops.

## What Changes

### `Command.fs` — remove 6 custom operations from `CommandBuilder`

Remove:
- `role`
- `objective`
- `instructions`
- `context`
- `output`
- `examples`

Keep:
- `name`, `description` (metadata)
- `section`, `import`, `importRaw`, `template`, `templateFile` (structural)
- `prompt` (composition)

## Test Impact

`CommandTests.fs` uses `role`, `instructions`, and `section` directly on `command { ... }` (line 44–49) and `instructions` again on line 88. These must be migrated to use `prompt { ... }` composition:

```fsharp
// Before
command {
    name "my-cmd"
    description "Test"
    role "You are a helper"
    instructions "Follow the steps"
    section "notes" "Extra notes here"
}

// After
command {
    name "my-cmd"
    description "Test"
    prompt (prompt {
        role "You are a helper"
        instructions "Follow the steps"
    })
    section "notes" "Extra notes here"
}
```

## Steps

1. Remove `role`, `objective`, `instructions`, `context`, `output`, `examples` from `CommandBuilder` in `src/FsAgent/Command.fs`
2. Update affected tests in `tests/FsAgent.Tests/CommandTests.fs` to use `prompt { ... }` composition
3. Run `dotnet build && dotnet test` — verify clean

## Files to Modify

| File | Change |
|------|--------|
| `src/FsAgent/Command.fs` | Remove 6 custom operations |
| `tests/FsAgent.Tests/CommandTests.fs` | Migrate direct content ops to `prompt` composition |
| `CHANGELOG.md` | Note breaking change under `[Unreleased]` |

## Breaking Change Note

This is a breaking change to the public DSL surface. Any caller using `command { role ... }`, `command { instructions ... }`, etc. must migrate to wrapping content in `prompt { ... }`.
