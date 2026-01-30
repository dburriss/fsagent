## Context

The FsAgent library provides a DSL for authoring agent files with an `import` operation that embeds external file content. Currently, `ImportInclusion.IncludeRaw` embeds content directly without any wrapper. This loses format context that could help consumers (AI agents, editors) understand the embedded content's structure.

The AST already tracks `DataFormat` (Yaml, Json, Toon, Unknown) for imported files via `inferFormat`. The writer can use this to derive appropriate code fence language tags.

## Goals / Non-Goals

**Goals:**
- Provide code-block-wrapped import that preserves format hints (` ```json `, ` ```yaml `, ` ```toon `)
- Maintain backward compatibility path via renamed `importRaw` operation
- Keep AST unchanged—wrapping is a writer concern, not an AST concern

**Non-Goals:**
- Changing AST structure (the `Imported` node remains as-is)
- Supporting custom language tags (use inferred format only)
- Transforming or parsing imported content (raw embedding only)

## Decisions

### Decision 1: Wrapping is a writer concern, not AST

The AST `Imported` node already stores `DataFormat`. Rather than adding a "wrap" flag to the AST, the writer's `ImportInclusion` option controls embedding style. This keeps the AST pure and format-agnostic.

**Alternative considered**: Add `ImportedRaw` vs `ImportedCodeBlock` node types. Rejected because it duplicates format inference logic and complicates the AST for a presentation concern.

### Decision 2: New `IncludeCodeBlock` inclusion mode

Add `IncludeCodeBlock` to `ImportInclusion` discriminated union. The writer maps `DataFormat` to language tag:
- `Yaml` → `yaml`
- `Json` → `json`
- `Toon` → `toon`
- `Unknown` → empty (no language tag, just triple backticks)

**Alternative considered**: Extend `IncludeRaw` with an optional wrapper parameter. Rejected because it overloads one mode's semantics.

### Decision 3: DSL `import` becomes code-block, `importRaw` for raw

Rename current `import` → `importRaw` (breaking change). New `import` produces same AST node, but the intended usage pairs with `IncludeCodeBlock` writer option. The DSL itself doesn't change behavior—both produce `Imported` nodes. The semantic difference is in writer configuration.

**Alternative considered**: Add `importCodeBlock` operation instead of renaming. Rejected because code-block wrapping is the better default behavior—raw embedding is the exception.

## Risks / Trade-offs

**[Risk] Breaking change for existing `import` users** → Mitigation: Document in release notes; rename is mechanical (`import` → `importRaw`).

**[Trade-off] DSL operation name doesn't enforce writer mode** → The DSL `import` vs `importRaw` is a naming convention; both produce identical AST nodes. Writer configuration must match intent. Accepted because AST purity is more valuable than coupling DSL to writer options.
