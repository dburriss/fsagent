## Context

`FsAgent.Writers.MarkdownWriter` is a misnomer: the module renders agents to Markdown, JSON, and YAML via `opts.OutputType`. The `write*` function prefix implies file I/O (side-effects), but all functions are pure string transformers. `writeMarkdown` is an undocumented alias for `writeAgent` that creates confusion.

This is a pre-1.0 breaking rename with no behavioral changes. The rename touches source, tests, examples, scripts, and documentation but leaves the AST, Options, OutputType, and OutputFormat types untouched.

## Goals / Non-Goals

**Goals:**
- Rename `MarkdownWriter` → `AgentWriter` in module declaration and `Library.fs` re-export
- Rename all public `write*` functions to `render*`
- Remove the redundant `writeMarkdown` alias
- Rename private helpers consistently (`writeMd/Json/Yaml` → `renderMd/Json/Yaml`)
- Update all call sites: tests, examples, scripts, docs, openspec artifacts
- Update `openspec/specs/markdown-writer/spec.md` to reference new symbol names
- Keep build and tests green throughout

**Non-Goals:**
- No behavioral changes — output is identical
- No `[<Obsolete>]` shims — clean break, pre-1.0
- No changes to `OutputType`, `OutputFormat`, `Options`, `WriterContext`, or `AgentHarness`
- No implementation of the composable `IWriter` hierarchy (future work)
- No renaming of the spec directory `openspec/specs/markdown-writer/` (historical record)

## Decisions

### Rename `write*` → `render*` (not `format*` or `serialize*`)

`render` is idiomatic for pure AST → string transformations in the F# ecosystem and matches the existing mental model (the module "renders" agents into text formats). `format` is ambiguous (often refers to string interpolation). `serialize` implies a reversible encoding, which is not the case here.

### Remove `writeMarkdown` rather than keep it

`writeMarkdown` duplicates `writeAgent` exactly. Keeping it means maintaining two names for one function with no semantic distinction. At pre-1.0, the cost of the break is low and the long-term clarity benefit is high.

### Private helper naming follows public naming convention

`writeMd`, `writeJson`, `writeYaml` become `renderMd`, `renderJson`, `renderYaml` for consistency. Although private, consistent naming reduces cognitive load when reading the module.

### Update `openspec/specs/markdown-writer/spec.md` symbol names in-place

The spec directory is not renamed (it is a historical artifact tied to its capability identity). The requirement text is updated to reference `AgentWriter`, `renderAgent`, `renderPrompt`, `renderCommand` where those names appear.

### Execution order: source first, then tests, then examples/docs

Changing source first causes compile errors in tests immediately, making it easy to verify the rename is complete before proceeding. Examples and docs are updated last as they have no compile impact.

## Risks / Trade-offs

- **Missed call site** → All references are caught by `dotnet build` for compiled code; docs/plans are text-only and require manual/grep sweep. Mitigation: grep for old names after build passes.
- **`writeMarkdown` removal breaks undocumented users** → Pre-1.0, acceptable. No backward-compat alias added by design.
- **Large diff across many files** → Increases review surface. Mitigation: atomic commits per execution step (source, tests, examples, docs) to keep each commit reviewable.

## Migration Plan

1. Rename module + functions in `src/FsAgent/Writers.fs`; remove `writeMarkdown`
2. Update `Library.fs` re-export
3. Update `.fsproj` compile include; rename `MarkdownWriterTests.fs` → `AgentWriterTests.fs` on disk
4. Update all test files — `dotnet build && dotnet test` must pass
5. Update examples (`agents-md.fsx`, `toon.fsx`, `typed-tools.fsx`) and `SmokeTest.fsx`
6. Update `ARCHITECTURE.md`, `README.md`, `CHANGELOG.md`, `MIGRATION.md`
7. Update `openspec/specs/markdown-writer/spec.md` symbol references
8. Sweep remaining internal docs (`CLAUDE.md`, `docs/`, `knowledge/`, `plans/`, `openspec/changes/`)

Rollback: revert commits in reverse order — no data migration, no external state.
