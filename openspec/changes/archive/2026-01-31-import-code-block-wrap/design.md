## Context

The FsAgent library provides a DSL for authoring agent files with an `import` operation that embeds external file content. Originally, `ImportInclusion.IncludeRaw` embedded content directly without any wrapper. This loses format context that could help consumers (AI agents, editors) understand the embedded content's structure.

The AST already tracks `DataFormat` (Yaml, Json, Toon, Unknown) for imported files via `inferFormat`. The writer can use this to derive appropriate code fence language tags.

## Goals / Non-Goals

**Goals:**
- Provide code-block-wrapped import that preserves format hints (` ```json `, ` ```yaml `, ` ```toon `)
- Make code-block wrapping the default for `import` without requiring writer configuration
- Maintain backward compatibility path via `importRaw` operation
- Allow users to disable code-block wrapping via writer option if needed

**Non-Goals:**
- Supporting custom language tags (use inferred format only)
- Transforming or parsing imported content (raw embedding only)

## Decisions

### Decision 1: Wrapping intent is stored in AST (revised)

Initially we considered keeping the AST unchanged and using writer options to control wrapping. However, this required users to configure both the DSL operation AND the writer, which was confusing.

**Final decision**: Add `wrapInCodeBlock: bool` to the `Imported` node. The `import` operation sets this to `true`, while `importRaw` sets it to `false`. The writer respects this flag by default.

**Alternative rejected**: Separate `ImportedRaw` vs `ImportedCodeBlock` node types. Adding a boolean flag is simpler and maintains a single `Imported` type.

### Decision 2: Simplified writer options

Remove `ImportInclusion` entirely—imports are always resolved. Add:
- `DisableCodeBlockWrapping`: bool (default false)

The writer respects the AST node's `wrapInCodeBlock` flag unless `DisableCodeBlockWrapping` is true.

**Rationale**: The DSL operation (`import` vs `importRaw`) should be the primary way to control wrapping, not writer options. This makes usage intuitive: `import` wraps, `importRaw` doesn't—no extra configuration needed. Excluding imports at the writer level is not a useful feature since users can simply omit the `import`/`importRaw` operations in their DSL if they don't want imports.

### Decision 3: DSL `import` vs `importRaw`

- `import "path"` → creates `Imported(path, format, wrapInCodeBlock=true)`
- `importRaw "path"` → creates `Imported(path, format, wrapInCodeBlock=false)`

Both produce `Imported` nodes but with different wrapping intent.

## Risks / Trade-offs

**[Risk] Breaking change for existing `import` users** → Mitigation: Document in release notes; users who want raw embedding switch to `importRaw`.

**[Risk] `ImportInclusion` removed entirely** → Imports are always resolved. Users who previously used `ImportInclusion=Exclude` should simply omit the `import`/`importRaw` operations in their DSL.

**[Trade-off] AST now includes presentation hint** → The `wrapInCodeBlock` flag is arguably a presentation concern. Accepted because it enables the simpler API where DSL operations fully control behavior without writer configuration.
