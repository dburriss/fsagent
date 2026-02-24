## Context

The `DataFormat` DU currently has four variants: `Yaml`, `Json`, `Toon`, `Unknown`. The `inferFormat` function maps file extensions to these variants. The `Unknown` variant is currently used as a catch-all that results in raw text output.

`.md` files imported via `importRaw` today fall through to `Unknown`, which accidentally works because `Unknown` produces raw text output. However, this conflates "we don't know the format" with "this is Markdown", making the intent unclear and leaving the door open for unintended format handling changes.

The `sectionFrom` operation does not yet exist. Currently, DSL users must write:
```fsharp
section "Build & Test" """...(large inline string)..."""
```
There is no way to load section content from an external file while keeping the section name supplied by the DSL.

## Goals / Non-Goals

**Goals:**
- Add `Markdown` as a first-class `DataFormat` variant so `.md` files are handled with explicit intent
- Add `sectionFrom` custom operation to all 4 CE builders that creates `Section(name, [Imported(path, inferFormat path, false)])`
- Ensure the writer renders `Markdown` imports as raw text (same behavior as today for `Unknown`)
- Provide a language tag of `"markdown"` if code-block wrapping is ever requested for a `Markdown` import

**Non-Goals:**
- No markdown structure parsing (section content is opaque text)
- No changes to how `import` (with code block wrapping) works
- No changes to `templateFile` behavior
- No new format-specific serialization for `Markdown`
- No changes to `Unknown` handling

## Decisions

### Decision 1: Add `Markdown` before `Unknown` in the DU

`Markdown` is inserted as a named variant before `Unknown`. The `formatToLanguageTag` function gains a `Markdown -> "markdown"` case. The `resolveImportedContent` function already handles all non-Toon formats by returning raw content — no special branch needed for `Markdown`.

**Alternatives considered:** Keeping `.md` as `Unknown` — rejected because it conflates two different intents and would silently break if `Unknown` handling ever changes.

### Decision 2: `sectionFrom` wraps in a `Section` node, not a bare `Imported` node

```fsharp
Section(name, [Imported(path, inferFormat path, false)])
```
This is consistent with how `section` works and ensures the heading is always emitted by the section renderer, not left to the file content.

**Alternatives considered:** Adding a new `SectionFromFile` AST node — rejected as unnecessary complexity; the `Section` + `Imported` composition already models this correctly.

### Decision 3: Add `sectionFrom` to all 4 builders in a single change

All 4 builders (`Prompt.fs`, `Agent.fs`, `Skill.fs`, `Command.fs`) expose a consistent surface. Leaving any builder without `sectionFrom` would create an inconsistent API.

## Risks / Trade-offs

- **[Risk]** If the imported file does not exist at write time, the writer emits `[Error loading <path>]`. → Mitigation: This matches the existing `import`/`importRaw` behavior; no special handling needed.
- **[Risk]** Adding `Markdown` to the DU is a non-breaking addition but any exhaustive match in external code (outside the library) would get a compile warning. → Mitigation: `DataFormat` is a library type; external exhaustive matches are uncommon and the warning is compile-time, not silent.
- **[Trade-off]** `sectionFrom` always sets `wrapInCodeBlock = false`. If a user wants a Markdown file wrapped in a code block, they use `import` instead.
