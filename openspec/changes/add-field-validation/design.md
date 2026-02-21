## Context

`Writers.fs` exposes three render functions: `renderAgent`, `renderCommand`, and `renderSkill`. `renderAgent` already has a Copilot-only guard (lines 496–502) that throws when `name` or `description` are absent from frontmatter. The guard uses a sequential fail-fast pattern: it checks `name.IsNone || desc.IsNone` in a single condition, so the message doesn't distinguish which field is missing. `renderCommand` and `renderSkill` have no validation at all — missing fields produce malformed or empty output silently.

## Goals / Non-Goals

**Goals:**
- Replace `renderAgent`'s single-condition guard with a collect-then-throw pattern that names each missing field individually.
- Add equivalent collect-then-throw validation to `renderCommand` (checks `cmd.Name`, `cmd.Description` record fields).
- Add equivalent collect-then-throw validation to `renderSkill` (checks `Frontmatter["name"]`, `Frontmatter["description"]` map entries).
- All three produce a bulleted error message listing every violation in one `failwith` call.

**Non-Goals:**
- No validation of `temperature`, `version`, `model`, or other optional fields.
- No changes to DSL types, builders, or AST structures.
- No migration of `Skill` from map-based to typed record.
- No `Result`-based error handling — `failwith` matches the existing pattern.

## Decisions

**Collect-then-throw over sequential guards**

Sequential `failwith` stops at the first violation; the caller must fix and re-run to discover the next error. Collecting all violations into a `string list` before a single `failwith` reports everything at once. This is strictly better UX at zero extra complexity.

```fsharp
let errors = [
    if condition1 then "error 1"
    if condition2 then "error 2"
]
if not errors.IsEmpty then
    failwith (header + "\n" + (errors |> List.map (sprintf "- %s") |> String.concat "\n"))
```

**Validation placement — start of render function, before any serialization**

Mirrors the existing `renderAgent` guard placement. Keeps validation co-located with the render logic without introducing a separate validation module (unnecessary for three simple field checks).

**`System.String.IsNullOrWhiteSpace` for string fields**

`cmd.Name` and `cmd.Description` are plain `string` record fields — `IsNullOrWhiteSpace` catches empty strings and whitespace-only values. For `Skill.Frontmatter` map entries, `Option.forall IsNullOrWhiteSpace` is true when the key is absent (`None`) or the value is blank, covering both the missing-key and blank-value cases in one expression.

**No harness-conditional path for `renderSkill`**

The `harness` parameter controls output format, not whether the skill is valid. Validation applies unconditionally — a skill without `name`/`description` is invalid regardless of target harness.

## Risks / Trade-offs

**Existing tests with blank fields will now fail** → Mitigation: Audit test files before shipping; update any test that intentionally uses blank fields for non-validation purposes (none expected given current test coverage).

**`Option.forall` semantics** → `Option.forall p None` returns `true` (vacuous truth), so a missing key correctly triggers the "requires non-empty" error. This is the right behaviour but is non-obvious; a code comment in the implementation should note it.

## Migration Plan

1. Update `renderAgent` validation block in `Writers.fs` — replace single-condition guard with collect pattern.
2. Add validation block to `renderCommand` in `Writers.fs` — insert before the `agentLike` construction.
3. Add validation block to `renderSkill` in `Writers.fs` — insert after `defaultOptions()` call, before frontmatter emission.
4. Add one new C test to `CommandTests.fs` asserting both field names appear when both are blank.
5. Add one new C test to `SkillTests.fs` asserting both field names appear when both are missing.
6. Run `dotnet build && dotnet test` — verify clean.

No rollback concern; this is an additive safety check with no external dependencies.

## Open Questions

None.
