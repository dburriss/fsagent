# Plan: Rename MarkdownWriter and write* functions

Status: Done

## Motivation

`MarkdownWriter` is misleading — the module already produces Markdown, JSON, and YAML output
via `opts.OutputType`. The `write*` prefix on public functions implies file I/O rather than
string transformation. `writeMarkdown` is a redundant alias for `writeAgent`.

## Rename Map

| Old | New | Visibility |
|---|---|---|
| `module MarkdownWriter` | `module AgentWriter` | public |
| `writeAgent` | `renderAgent` | public |
| `writePrompt` | `renderPrompt` | public |
| `writeCommand` | `renderCommand` | public |
| `writeMarkdown` | removed | public alias |
| `writeMd` | `renderMd` | private |
| `writeJson` | `renderJson` | private |
| `writeYaml` | `renderYaml` | private |

No `[<Obsolete>]` shim — pre-1.0, clean break.

## Execution Order

1. **`src/FsAgent/Writers.fs`** — rename module + all 7 functions; remove `writeMarkdown` alias
2. **`src/FsAgent/Library.fs`** — update re-export alias to `AgentWriter`
3. **`FsAgent.Tests.fsproj`** — update `<Compile Include>` to `AgentWriterTests.fs`
4. **Rename file on disk** — `MarkdownWriterTests.fs` → `AgentWriterTests.fs`
5. **Test files** — replace all `MarkdownWriter.*` → `AgentWriter.*`, `write*` → `render*`
   - `tests/FsAgent.Tests/AgentWriterTests.fs`
   - `tests/FsAgent.Tests/CommandTests.fs`
   - `tests/FsAgent.Tests/PromptTests.fs`
   - `tests/FsAgent.Tests/AgentPromptIntegrationTests.fs`
6. `dotnet build && dotnet test` — must be green before continuing
7. **Examples/scripts**
   - `examples/agents-md.fsx`
   - `examples/toon.fsx`
   - `examples/typed-tools.fsx`
   - `SmokeTest.fsx`
8. **`ARCHITECTURE.md`** — two changes:
   - Update module diagram (`MarkdownWriter` → `AgentWriter`, `writeAgent/Prompt` → `renderAgent/Prompt`)
   - Move the unimplemented `IWriter` / composable writer sections (1–3, lines 70–123) to a
     new **"Future: Composable Writer Hierarchy"** section at the bottom; replace with a
     factual description of the actual `AgentWriter` module and `Options`-based dispatch
9. **`README.md`** — update all references
10. **`CHANGELOG.md`** — add entry for rename; update historical references
11. **`MIGRATION.md`** — add migration note for all renamed symbols
12. **Internal docs** (textual only, no compile impact):
    - `CLAUDE.md`
    - `docs/using-ast.md`
    - `examples/README.md`
    - `knowledge/fsagent-api.md`
    - `plans/*.md` (all 7 existing plan files)
    - `openspec/specs/**/*.md`
    - `openspec/changes/add-slash-command-dsl/**/*.md`
    - `openspec/changes/archive/**/*.md` (low priority — historical records)

## Reference Counts (for scope awareness)

| Symbol | Total references |
|---|---|
| `MarkdownWriter` | ~100 across all files |
| `writeAgent` | 105 |
| `writePrompt` | 61 |
| `writeCommand` | 42 |
| `writeMarkdown` | ~25 |
| `writeMd` | 50 (mostly plans/openspec) |
| `writeJson` | 9 (mostly plans/openspec) |
| `writeYaml` | 9 (mostly plans/openspec) |

## Non-Goals

- Do not rename `writeAgent` → `renderAgent` in `Options.CustomWriter` signature — wait, that
  field takes `Agent -> Options -> string`, not a named function reference; no impact.
- Do not change `OutputType`, `OutputFormat`, `Options`, `WriterContext`, `AgentHarness`.
- Do not implement the composable `IWriter` hierarchy — that is captured as future work.
- Do not add backward-compat aliases.
