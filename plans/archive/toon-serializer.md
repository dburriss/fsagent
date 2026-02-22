# Plan: TOON Serializer

Status: Done

## Description

Implement a TOON serializer module (`ToonSerializer.fs`) so that `.toon` imported files are parsed and re-emitted via a proper serializer rather than passed through as raw bytes. TOON is a real, distinct format ([spec](https://github.com/0xZunia/ToonSharp/blob/main/SPEC.md)) — line-oriented, LLM-optimized, with its own syntax for arrays and tabular data. It is **not** YAML.

A .NET implementation exists: [`ToonSharp`](https://github.com/0xZunia/ToonSharp) on NuGet.

**Note:** The existing `examples/toon-data.toon` uses YAML syntax — that file is not valid TOON per the spec and will need to be updated as part of this work.

---

## Scope

### In scope
1. Add `ToonSharp` NuGet dependency to `FsAgent.fsproj`.
2. Create `src/FsAgent/ToonSerializer.fs` — low-level module wrapping `ToonSharp` with an F#-friendly API.
3. Update `Writers.fs`: wire `ToonSerializer` into the `Imported` node handler in `renderMd` and `renderSkill` for `DataFormat.Toon` files, replacing the raw `File.ReadAllText` passthrough.
4. Update `examples/toon-data.toon` to valid TOON syntax (or add a new valid example).
5. Add `C`-category tests for the TOON import path (file read + parse + embed boundary).
6. Update `CHANGELOG.md`.

### Out of scope (deferred)
- YAML→TOON and JSON→TOON cross-format re-serialization at write time (separate backlog item).
- JSON→YAML and YAML→JSON conversion.

---

## Dependencies / Prerequisites

- `ToonSharp` NuGet package (C# library, .NET 9 compatible; callable from F#).
- No AST changes needed.
- No existing TOON serializer to migrate from.

---

## Design

### `ToonSerializer.fs` (new file — Low-Level Utilities layer)

```fsharp
module FsAgent.ToonSerializer

open ToonSharp

/// Parse TOON string and re-emit as normalized TOON.
/// Returns Ok(toonText) or Error(message) on failure.
let serialize (raw: string) : Result<string, string> =
    try
        let node = ToonSerializer.Deserialize(raw)
        Ok (ToonSerializer.Serialize(node))
    with
    | :? ToonException as ex -> Error $"Line {ex.LineNumber}: {ex.Message}"
    | ex -> Error ex.Message
```

This is a parse-then-reserialize round-trip: validates the input and produces normalized output. The `"toon"` code fence language tag is unchanged — already handled by `formatToLanguageTag` in `Writers.fs:141`.

### Integration point in `Writers.fs`

Both `renderMd/writeNode` (~line 454) and `renderSkill/writeNode` (~line 667) have identical `Imported` handler logic. Extract a shared helper first, then add format dispatch:

```fsharp
let resolveImportedContent (path: string) (format: DataFormat) : string =
    let raw = System.IO.File.ReadAllText(path)
    match format with
    | DataFormat.Toon ->
        match ToonSerializer.serialize raw with
        | Ok s -> s
        | Error msg -> $"[TOON parse error: {msg}]\n{raw}"
    | _ -> raw   // YAML/JSON/Unknown: raw passthrough (current behaviour)
```

This change is **backward-compatible** — YAML/JSON/Unknown imports are unaffected.

### File order in `.fsproj`

`ToonSerializer.fs` must be compiled before `Writers.fs`. Insert it after `AST.fs` and before `Writers.fs`.

---

## Impact on Existing Code

| File | Change |
|---|---|
| `src/FsAgent/FsAgent.fsproj` | Add `ToonSharp` package reference; add `ToonSerializer.fs` compile item |
| `src/FsAgent/ToonSerializer.fs` | New module |
| `src/FsAgent/Writers.fs` | Extract `resolveImportedContent` helper; add `DataFormat.Toon` dispatch |
| `examples/toon-data.toon` | Rewrite to valid TOON syntax |
| `tests/FsAgent.Tests/` | New or extended `C`-category tests |
| `CHANGELOG.md` | Add entry |

No breaking API changes. Existing YAML/JSON import behavior is preserved.

---

## Acceptance Criteria

- [ ] `ToonSharp` is added as a NuGet dependency and builds cleanly.
- [ ] `ToonSerializer.serialize` round-trips a valid `.toon` file without error.
- [ ] The `wrapInCodeBlock = true` path embeds the serialized content in ` ```toon ` fences.
- [ ] A malformed `.toon` file produces an inline error message; no exception propagates.
- [ ] YAML/JSON/Unknown import behavior is unchanged (regression-free).
- [ ] `examples/toon-data.toon` is valid TOON syntax.
- [ ] All existing tests pass; new `C`-category tests cover happy path and error path.

---

## Testing Strategy

**A-category (acceptance):** The existing TOON acceptance test (`AgentWriterTests.fs:144–154`) tests fence wrapping. Extend it to assert the embedded content is valid TOON (key presence after parse, not raw YAML).

**C-category (boundary) — new tests labelled `[<Trait("category","toon")>]`:**
- Parse `examples/toon-data.toon`, serialize, assert expected keys present.
- Feed malformed TOON string, assert `Error` is returned and output contains error marker.
- Assert `File.ReadAllText` path still used unchanged for `DataFormat.Yaml` (no regression).

Run targeted: `dotnet test --filter "category=toon"`

---

## Risks and Mitigations

| Risk | Mitigation |
|---|---|
| `ToonSharp` is C#; interop friction in F# | The API is simple (`Serialize`/`Deserialize` statics) — minimal friction expected |
| `ToonSharp` targets .NET 9; project may differ | Verify `FsAgent.fsproj` target framework; adjust if needed |
| Existing `.toon` example is YAML syntax, not valid TOON | Update example as part of this task; add note to `CHANGELOG.md` |
| Duplicate `Imported` logic in `renderMd` and `renderSkill` | Extract `resolveImportedContent` helper to fix both in one place |

---

## Open Questions

None.
