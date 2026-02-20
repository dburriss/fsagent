## Context

FsAgent already provides a DSL for `Agent`, `Prompt`, and `SlashCommand` types, each with a corresponding record, computation expression builder, and writer function in `AgentWriter`. All three follow the same stratified pattern:

1. **AST layer** – `FsAgent.AST.Node` for content nodes (shared)
2. **Domain layer** – a record type in its own namespace (e.g., `FsAgent.Commands.SlashCommand`)
3. **Builder layer** – a CE in an `[<AutoOpen>]` module exposing a named value (`agent`, `prompt`, `command`)
4. **Writer layer** – `AgentWriter.render*` functions that convert the record to a string
5. **Library layer** – `Library.fs` re-exports type aliases and DSL values for ergonomic top-level use

The `Skill` feature extends this same pattern without modifying any existing types.

SKILL.md is a Markdown file with YAML frontmatter (similar to agent files) and free-form Markdown body sections. It is used by OpenCode and similar tools to load structured skill instructions at runtime.

## Goals / Non-Goals

**Goals:**
- Add a `Skill` record type in a new `FsAgent.Skills` namespace.
- Add a `skill { ... }` CE with standard custom operations (`name`, `description`, `license`, `compatibility`, `metadata`, `section`, `prompt`, `import`, `importRaw`, `template`, `templateFile`).
- Add `AgentWriter.renderSkill` that produces a SKILL.md-compatible Markdown string with YAML frontmatter.
- Expose `Skill` type alias and `skill` CE in `Library.fs` alongside `Agent`, `Prompt`, `SlashCommand`.
- No breaking changes to existing public API.

**Non-Goals:**
- New `OutputType` variants or JSON/YAML output for skills (Markdown only).
- New `AgentHarness` values for skill rendering.
- Changes to `Agent`, `Prompt`, `SlashCommand`, or `Node`.
- CLI tooling for skill generation.

## Decisions

### 1. Reuse `Node list` for skill body sections

**Decision**: `Skill.Sections` is `Node list`, identical to `Agent.Sections` and `SlashCommand.Sections`.

**Rationale**: Skills have the same content model as agents and commands — headings, text, imports, and templates. Reusing `Node` avoids a parallel type hierarchy and lets `renderMd` be shared with trivial adaptation.

**Alternative considered**: A dedicated `SkillSection` discriminated union. Rejected — adds complexity with no benefit given the content model is identical.

---

### 2. Frontmatter as `Map<string, obj>`, consistent with `Agent`

**Decision**: `Skill` uses `Frontmatter: Map<string, obj>` for all frontmatter fields, identical to `Agent`.

**Rationale**: Consistency with the existing pattern. The CE builder exposes named custom operations (`name`, `description`, `license`, `compatibility`, `metadata`) that write into the map under their conventional keys — same ergonomics, no bespoke record fields.

**Alternative considered**: Typed record fields per SKILL.md key. Deferred — would offer compile-time safety for fixed keys but diverges from the `Agent` pattern without sufficient justification at this stage.

---

### 3. `renderSkill` as a dedicated function, not an overload of `renderAgent`

**Decision**: Add `AgentWriter.renderSkill` that accepts a `Skill` record and an `Options -> unit` configurator.

**Rationale**: `Skill` frontmatter serialization differs from `Agent` (no `tools`/`disallowedTools`, different keys). A dedicated function keeps the rendering logic explicit and avoids conditional branching inside `renderMd` for a type it shouldn't know about.

**Alternative considered**: Convert `Skill` to `Agent` internally and pass to `renderAgent`. Rejected — frontmatter structure is different enough that adapting it cleanly would require the same amount of code, with worse encapsulation.

---

### 4. `renderSkill` accepts `AgentHarness` for template tool-name resolution

**Decision**: `renderSkill` accepts an `AgentHarness` parameter (defaulting to `Opencode` when not specified) so `Template` and `TemplateFile` nodes resolve `{{{tool X}}}` correctly for the target harness.

**Rationale**: Accepting harness now keeps `renderSkill` consistent with `renderAgent`/`renderCommand` and avoids a breaking signature change later. The cost is a single extra parameter that callers can ignore by passing `AgentWriter.Opencode`. Skills may reference tools in their instruction text, so harness-aware resolution is genuinely useful.

**Alternative considered**: Hard-code `Opencode` internally and add harness support later. Rejected — the pattern is already established; deferring just creates a future breaking change.

---

### 5. `Skill.fs` placed after `Command.fs` in project file

**Decision**: Compile order: `AST.fs` → `Tools.fs` → `Prompt.fs` → `Agent.fs` → `Command.fs` → `Skill.fs` → `Writers.fs` → `Library.fs`.

**Rationale**: `Skill.fs` depends on `FsAgent.AST` and `FsAgent.Prompts`. It does not depend on `Agent` or `Command`. Placing it after `Command.fs` and before `Writers.fs` matches the existing ordering convention and keeps the dependency graph acyclic.

## Risks / Trade-offs

- **Frontmatter serialization duplication**: `renderSkill` will contain YAML frontmatter rendering logic similar to `renderMd`. This is a known trade-off of the dedicated-function approach (Decision 3). Mitigation: extract a small private helper for YAML scalar serialization that both functions can share.

- **`Metadata` map type**: Using `Map<string, obj>` for `Metadata` allows arbitrary sub-keys but loses type safety for common ones like `author` and `version`. Accepted — the metadata sub-schema is informally defined by tool conventions, so flexibility is more useful than strictness here.

- **Template nodes without harness**: resolved — `renderSkill` now accepts `AgentHarness`, so tool-name substitution in `Template`/`TemplateFile` nodes works correctly for any target harness.

## Open Questions

- Should `renderSkill` accept an `AgentHarness` for future tool-name resolution in template nodes? **Resolved** — yes, accepted upfront (Decision 4).
- Should `Metadata` be a typed record (`SkillMetadata`) instead of `Map<string, obj>`? Deferred — the field set is informal and may vary by tool.
