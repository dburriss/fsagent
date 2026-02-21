## Context

`Writers.fs` exposes three render functions: `renderAgent`, `renderCommand`, and `renderSkill`. `renderAgent` already has a Copilot-only guard (lines 496–502) that throws when `name` or `description` are absent from frontmatter. The guard uses a sequential fail-fast pattern: it checks `name.IsNone || desc.IsNone` in a single condition, so the message doesn't distinguish which field is missing. `renderCommand` and `renderSkill` have no validation at all — missing fields produce malformed or empty output silently.

## Goals / Non-Goals

**Goals:**
- Replace `renderAgent`'s single-condition guard with a collect-then-throw pattern that names each missing field individually.
- Add equivalent collect-then-throw validation to `renderCommand` (checks `cmd.Name`, `cmd.Description` record fields).
- Add equivalent collect-then-throw validation to `renderSkill` (checks `Frontmatter["name"]`, `Frontmatter["description"]` map entries).
- All three raise a custom `ValidationException` with a bulleted error message listing every violation.

**Non-Goals:**
- No validation of `temperature`, `version`, `model`, or other optional fields.
- No changes to DSL types, builders, or AST structures.
- No migration of `Skill` from map-based to typed record.
- No `Result`-based error handling — custom exception matches F# idiomatic writer-time errors.

## Decisions

**Collect-then-throw over sequential guards**

Sequential guards stop at the first violation; the caller must fix and re-run to discover the next error. Collecting all violations into a `string list` before a single `raise` reports everything at once.

```fsharp
let errors = [
    if condition1 then "error 1"
    if condition2 then "error 2"
]
if not errors.IsEmpty then
    raise (ValidationException (header + "\n" + (errors |> List.map (sprintf "- %s") |> String.concat "\n")))
```

**Custom `ValidationException` over `failwith`**

`failwith` raises a plain `System.Exception`, which callers cannot distinguish from other runtime failures. A custom F# exception type (`exception ValidationException of string`, defined in `FsAgent.Writers`) lets callers catch only validation errors explicitly. This is the idiomatic F# approach and mirrors how structured error handling is done in larger F# codebases. Defined in `Writers.fs` (co-located with the validation that raises it) — no separate file needed for three call sites.

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

1. Define `exception ValidationException of string` near the top of `AgentWriter` module in `Writers.fs`.
2. Update `renderAgent` validation block — replace `failwith` with `raise (ValidationException ...)`.
3. Add validation block to `renderCommand` — `raise (ValidationException ...)`.
4. Add validation block to `renderSkill` — `raise (ValidationException ...)`.
5. Update tests to catch `ValidationException` instead of `System.Exception`.
6. Run `dotnet build && dotnet test` — verify clean.

No rollback concern; this is an additive safety check with no external dependencies.

## Open Questions

None.
