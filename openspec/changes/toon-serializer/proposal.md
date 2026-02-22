## Why

`.toon` imported files are currently passed through as raw bytes without any parsing or validation. TOON is a distinct, LLM-optimized format — not YAML — and deserves proper parse-and-reserialize treatment to validate content and produce normalized output.

## What Changes

- Add `ToonSharp` NuGet dependency to `FsAgent.fsproj`
- New module `ToonSerializer.fs` wrapping `ToonSharp` with an F#-friendly `serialize` API
- `Writers.fs`: extract shared `resolveImportedContent` helper; dispatch `DataFormat.Toon` through the new serializer instead of raw passthrough
- Update `examples/toon-data.toon` from invalid YAML syntax to valid TOON syntax
- Add `C`-category tests covering happy path, error path, and YAML regression

## Capabilities

### New Capabilities
- `toon-serializer`: Parse and re-serialize TOON-format imported data files; surface parse errors inline without propagating exceptions; leave YAML/JSON/Unknown imports unaffected

### Modified Capabilities

## Impact

- `src/FsAgent/FsAgent.fsproj`: new `ToonSharp` package reference and `ToonSerializer.fs` compile item
- `src/FsAgent/ToonSerializer.fs`: new file (must compile before `Writers.fs`)
- `src/FsAgent/Writers.fs`: extract `resolveImportedContent`; add `DataFormat.Toon` dispatch
- `examples/toon-data.toon`: rewritten to valid TOON syntax
- `tests/FsAgent.Tests/`: new `C`-category tests (`[<Trait("category","toon")>]`)
- External dependency: `ToonSharp` NuGet package (C# library, .NET compatible)
