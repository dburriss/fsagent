## Context
Introduce a top-level, flavour-agnostic F# computation expression (CE) `agent { ... }` that constructs the Agent AST. The DSL must remain pure and independent of writer flavours (OpenCode, Copilot, Claude). Common frontmatter elements (description, model, temperature, tools, optional name) should be expressible ergonomically while remaining generic.

## Goals / Non-Goals
- Goals: Simple CE ergonomics; deterministic AST; flavour-agnostic frontmatter; immutable nodes; composable sections; import references as path-only with inferred format
- Non-Goals: Writer-specific frontmatter enforcement; runtime I/O in DSL; flavour coupling; declaring required frontmatter keys in DSL

## Decisions
- Use CE to sequence section and metadata construction in a readable form
- Provide DSL `meta { ... }` builder that writes a generic frontmatter map:
  - `kv: string -> string -> unit`
  - `kvList: string -> string list -> unit`
  - `kvObj: string -> Map<string, obj> -> unit`
  - `kvListObj: string -> obj list -> unit`
- Provide generic section builder `section name content` for ad hoc sections alongside core section ops
- Infer import format from filename extensions (`.yml/.yaml → yaml`, `.json → json`, `.toon → toon`); unknown extensions set format to `unknown`, writers may decide handling
- Keep AST constructors as the authoritative creation functions; DSL delegates to them (stratified design)
- Maintain deterministic order: frontmatter first, then user-declared sections

## Alternatives Considered
- MetaValue discriminated union: more explicit typing, but higher complexity; deferred in favor of simpler `obj` maps/lists that align with AST helpers
- Embedding flavour-specific keys in DSL: rejected; writers translate generic frontmatter to flavour conventions

## Risks / Trade-offs
- Ambiguity in `obj` values → Mitigated by writer-side validation and tests; DSL remains minimal
- CE complexity → Keep the surface minimal; prefer small combinators
- Import inference ambiguity → Provide clear extension mapping and an `unknown` fallback

## Migration Plan
- Additive: existing AST and writers remain valid; new DSL compiles to the same AST

## Resolved Decisions
- Identity semantics: `name` is optional across flavours; writers interpret accordingly (OpenCode uses filename; Copilot/Claude use `name`)
- Frontmatter requirements: enforcement delegated to writers in future specs; DSL remains optional and flavour‑agnostic
- Tool typing: tools are strings; advanced/MCP metadata expressed via `kvObj`/`kvListObj`
