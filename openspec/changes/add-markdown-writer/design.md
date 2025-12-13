## Context
We need a first writer to convert the immutable Agent AST to Markdown. The writer must be pure relative to the AST (no AST mutations) and provide configuration for imported data inclusion and heading renames/formatting. Output must support multiple agent formats.

## Goals / Non-Goals
- Goals: Provide a Markdown writer with configurable options; deterministic output; simple defaults; familiar configuration pattern using mutable options object inside a function; support `opencode` and `copilot` output formats; allow custom writers; support multiple output types (`md|json|yaml`).
- Non-Goals: Implement HTML writer, or file I/O. Avoid complex templating until requested.

## Decisions
- Writer API: `writeMarkdown(agent, configure: Options -> unit)` where `configure` mutates an `Options` record/class; returns `string`.
- Options: 
  - `outputFormat: AgentFormat` (values: `opencode | copilot`; default `opencode`)
  - `outputType: OutputType` (values: `md | json | yaml`; default `md`)
  - `importInclusion: ImportInclusion` (values: `none | raw`; default `none`) — raw inserts transformed content, no automatic code fences
  - `renameMap: Map<string,string>` (section name -> display heading)
  - `headingFormatter: (name:string -> string) option` — optional global formatter applied after renames; default identity
  - `generatedFooter: (context: WriterContext -> string) option` — when provided, appends returned string to output (default none)
  - `includeFrontmatter: bool` — when true and format supports frontmatter, emit agent metadata frontmatter; default `true`
  - `customWriter: (agent: AgentAst -> opts: Options -> string) option` — when set, overrides built-in writer logic
- Heading style: Fixed to ATX (`#`, `##`, ...) for all `md` outputs (no Setext in this change). For `json`/`yaml`, headings aren’t applicable; use structured keys.
- Default behavior: `outputFormat=opencode`, `outputType=md`, ATX headings, `importInclusion=none`, no renames, default heading formatter (identity), `includeFrontmatter=true`, no footer, deterministic order per `agent-ast` traversal.
- Imported data handling: The AST stores only references (path + format). Writer may embed raw transformed content (when `importInclusion=raw`) without additional wrapping; authors can explicitly add code fences if desired. Reference pattern in `knowledge/import-data.md` and `knowledge/import-data.rules.json` for examples.
- Copilot frontmatter: Emit minimum fields `name` and `description` when `outputFormat=copilot` and `includeFrontmatter=true`.
- OpenCode frontmatter: Emit `description` only when `outputFormat=opencode` and `includeFrontmatter=true`.

## WriterContext
- Fields: `format: AgentFormat`, `outputType: OutputType`, `timestamp: DateTime`, `agentName: string option`, `agentDescription: string option`
- Purpose: Provide minimal context to the `generatedFooter` function.

## Alternatives considered
- Pure functional options (immutable) with a builder returning a new options value — rejected for usability parity with common .NET configuration patterns.
- Template-based writer — deferred; initial implementation should be minimal and deterministic.

## Risks / Trade-offs
- Mutable options in configuration increases API surface for misuse; mitigated by clear defaults and validation.
- Copilot format specifics may require additional constraints; mitigate by starting with minimum frontmatter and iterating.

## Migration Plan
- Introduce writer and options alongside existing AST, with tests.
- No changes to AST; only consumption.

## Open Questions
- Confirm any additional Copilot format conventions (beyond `name` and `description`).
