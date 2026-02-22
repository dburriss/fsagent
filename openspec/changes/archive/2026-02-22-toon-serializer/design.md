## Context

`Writers.fs` contains two identical `Imported` node handlers (`renderMd` ~line 454, `renderSkill` ~line 667) that read imported files with `File.ReadAllText` and pass content through unchanged regardless of format. For `DataFormat.Toon`, this means invalid or unnormalized TOON content is silently embedded. The existing `examples/toon-data.toon` uses YAML syntax and is not valid TOON per the spec.

`ToonSharp` is a .NET C# library providing static `Serialize`/`Deserialize` methods. F# can call it directly with no interop adapter needed beyond exception mapping.

## Goals / Non-Goals

**Goals:**
- Parse and re-serialize `DataFormat.Toon` imports via `ToonSharp`, normalizing output and surfacing errors inline
- Eliminate the duplicated `Imported` handler logic by extracting `resolveImportedContent`
- Keep YAML/JSON/Unknown import behaviour unchanged (raw passthrough)

**Non-Goals:**
- Cross-format conversion (YAML→TOON, JSON→TOON) — deferred
- Changes to the AST or the `DataFormat` discriminated union
- Altering the code-fence wrapping logic (language tag already correct)

## Decisions

### 1. Thin F# wrapper module over ToonSharp statics

`ToonSerializer.fs` exposes a single `serialize : string -> Result<string, string>` function. It calls `ToonSharp.ToonSerializer.Deserialize` then `ToonSharp.ToonSerializer.Serialize` (round-trip). Exceptions are caught and mapped to `Error` strings.

**Alternatives considered:**
- Inline the ToonSharp calls directly in `Writers.fs` — rejected; mixes I/O boundary with format logic and makes unit testing harder
- Expose `parse` and `serialize` separately — unnecessary complexity; callers only need the normalized string

### 2. Extract `resolveImportedContent` helper in Writers.fs

Both `renderMd` and `renderSkill` have byte-for-byte identical `Imported` handlers. Extract:

```fsharp
let resolveImportedContent (path: string) (format: DataFormat) : string =
    let raw = System.IO.File.ReadAllText(path)
    match format with
    | DataFormat.Toon ->
        match ToonSerializer.serialize raw with
        | Ok s -> s
        | Error msg -> $"[TOON parse error: {msg}]\n{raw}"
    | _ -> raw
```

Both handlers call `resolveImportedContent` instead of `File.ReadAllText`. The wrapping/fence logic is unchanged.

**Error strategy:** On parse failure, embed an inline error marker followed by the raw content. This makes errors visible in the generated file without propagating exceptions — consistent with the existing `[Error loading {path}]` pattern.

### 3. File compilation order

`ToonSerializer.fs` is inserted in `FsAgent.fsproj` after `AST.fs` and before `Writers.fs`. F# requires strict top-to-bottom compilation order.

### 4. Update examples/toon-data.toon to valid TOON syntax

The existing example uses YAML key-value syntax. It must be rewritten to valid TOON per the spec so the new serializer round-trips it without error. A minimal valid example with a few key-value entries and one array is sufficient.

## Risks / Trade-offs

- **ToonSharp targets .NET 9; project may target a different TFM** → verify `<TargetFramework>` in `FsAgent.fsproj` before adding the package; `net9.0` is expected but if lower, check NuGet compatibility
- **ToonSharp API shape** → the plan assumes static `Deserialize`/`Serialize` methods matching ToonSharp README; confirm actual public API after adding the package, adjust wrapper if method names differ
- **Duplicate handler removal introduces a shared code path** → the extracted helper is pure (modulo file I/O); failure modes are the same as before, just centralized; low risk
- **Inline error marker changes embedded output for malformed TOON** → this is intentional and acceptable; no existing tests assert on malformed TOON content
