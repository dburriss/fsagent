## Context

`Writers.fs` converts the AST to text output via two render functions: `renderMd` (for agents/prompts/commands) and `renderSkill`. Both share identical `Section` node handling that emits Markdown headings. The AST `Section` node is format-agnostic; rendering is a pure writer concern. `AgentWriter.Options` already carries per-render configuration (`RenameMap`, `HeadingFormatter`, `OutputType`), making it the natural place to add style control.

## Goals / Non-Goals

**Goals:**
- Allow callers to opt in to XML tag rendering for `Section` nodes via a single `Options` field
- Keep default behaviour (Markdown headings) completely unchanged
- Apply existing `RenameMap` and `HeadingFormatter` transforms to XML tag names

**Non-Goals:**
- No per-section style control (no mixed Markdown + XML in one document)
- No XML escaping of section content (verbatim, same as Markdown today)
- No AST changes — `Section` node is unchanged
- No JSON/YAML renderer changes (`nodeToObj` is format-neutral)
- No `CustomWriter` signature changes
- No XML name sanitisation — valid element names are caller's responsibility

## Decisions

### Add `SectionStyle` as a new DU alongside `OutputType`

Both are rendering concerns orthogonal to the AST. Placing them together in `Writers.fs` keeps writer configuration co-located and makes the pattern discoverable for future rendering options.

Alternatives considered:
- Boolean flag `UseXmlSections` — less extensible; a DU is idiomatic F# and allows future cases (e.g., `Html`) without a breaking change.
- Separate writer type per style — overkill; the branch is two lines and the rest of the render pipeline is identical.

### Duplicate the branch in both `renderMd` and `renderSkill`

Both functions have their own `writeNode` closure, so the branch must appear in each. The duplication is small (one `match` expression) and avoids coupling the two render paths through a shared helper that would complicate their individual closure environments.

### XML mode omits level-based blank-line guards

The blank-line insertion before top-level sections in Markdown mode is a Markdown formatting concern (separating frontmatter from content). In XML mode, nesting is conveyed by tag structure alone; inserting blank lines would add noise.

## Risks / Trade-offs

- `HeadingFormatter` can produce invalid XML element names — no mitigation added; documented as caller's responsibility to keep the API surface minimal.
- `renderSkill` is a second code path that must be updated in sync; risk of divergence on future `Section` changes → mitigated by co-locating the DU and keeping the branch identical in both functions, making diff review straightforward.

## Open Questions

None — scope is fully defined by the plan.
