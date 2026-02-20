## Context

`CommandBuilder` currently exposes two categories of operations: structural operations (`section`, `import`, `importRaw`, `template`, `templateFile`, `prompt`) and semantic content operations (`role`, `objective`, `instructions`, `context`, `output`, `examples`). The semantic ops are a verbatim copy of what `PromptBuilder` provides, creating a duplicated API surface.

The `prompt` operation on `CommandBuilder` already provides a clean composition hook — callers can wrap content in `prompt { ... }` and pass it to a command. The duplicate ops exist as a shortcut but violate the single-responsibility boundary between the two builders.

## Goals / Non-Goals

**Goals:**
- Remove the 6 duplicate semantic content operations from `CommandBuilder`
- Preserve all structural and metadata operations unchanged
- Migrate existing test usages to `prompt { ... }` composition
- Record the breaking change in `CHANGELOG.md`

**Non-Goals:**
- Changing `AgentBuilder` (it does not have these ops)
- Changing `PromptBuilder` in any way
- Modifying writer or AST layers
- Introducing any new DSL features

## Decisions

### Remove ops directly — no deprecation cycle

The codebase is pre-1.0 and the tests are the only known callers. A deprecation wrapper would just defer the cleanup with no benefit. Remove directly and migrate the tests in the same change.

**Alternative considered**: Add `[<Obsolete>]` attributes first, then remove in a follow-up. Rejected — unnecessary ceremony for an internal-only surface at this stage.

### `prompt` inline syntax is the migration path

The existing `prompt` custom operation on `CommandBuilder` already inlines the prompt's sections directly into the command's section list (`cmd.Sections @ prompt.Sections`). This means:

```fsharp
// Before
command {
    role "You are a helper"
    instructions "Follow the steps"
}

// After
command {
    prompt (prompt {
        role "You are a helper"
        instructions "Follow the steps"
    })
}
```

No AST changes are required — the output is identical.

### No changes to `SlashCommand` record

The `SlashCommand` type uses a flat `Sections: Node list`. The semantic ops were just appending `Prompt.*` nodes to that list, exactly as `prompt` composition does. The record shape is correct as-is.

## Risks / Trade-offs

- **BREAKING change to public DSL** → Documented in `CHANGELOG.md`; callers must wrap content in `prompt { ... }`. The migration is mechanical and the pattern is already supported.
- **Test churn** → Two test locations in `CommandTests.fs` need updating. Low risk — the test logic is unchanged, only the DSL call site.
