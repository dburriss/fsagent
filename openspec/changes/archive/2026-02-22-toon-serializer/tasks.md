## 1. Dependencies & Project Setup

- [x] 1.1 Verify `<TargetFramework>` in `src/FsAgent/FsAgent.fsproj` is `net9.0` (or confirm ToonSharp NuGet compatibility with actual TFM)
- [x] 1.2 Add `ToonSharp` NuGet package reference to `src/FsAgent/FsAgent.fsproj`
- [x] 1.3 Add `ToonSerializer.fs` compile item to `FsAgent.fsproj` after `AST.fs` and before `Writers.fs`
- [x] 1.4 Run `dotnet build` and confirm clean build with new dependency

## 2. ToonSerializer Module

- [x] 2.1 Create `src/FsAgent/ToonSerializer.fs` with module header `module FsAgent.ToonSerializer`
- [x] 2.2 Confirm actual public API of `ToonSharp` (static method names: `Deserialize`/`Serialize`, exception type)
- [x] 2.3 Implement `serialize : string -> Result<string, string>` — call `Deserialize` then `Serialize`, catch `ToonException` (map to `Error "Line N: msg"`) and generic exceptions (map to `Error msg`)
- [x] 2.4 Run `dotnet build` and confirm `ToonSerializer.fs` compiles cleanly

## 3. Writers.fs Refactor

- [x] 3.1 Extract `resolveImportedContent (path: string) (format: DataFormat) : string` helper in `Writers.fs` — raw passthrough for YAML/JSON/Unknown, `ToonSerializer.serialize` dispatch for `DataFormat.Toon` with inline error marker on failure
- [x] 3.2 Replace `System.IO.File.ReadAllText(path)` in `renderMd` `Imported` handler with `resolveImportedContent path format`
- [x] 3.3 Replace `System.IO.File.ReadAllText(path)` in `renderSkill` `Imported` handler with `resolveImportedContent path format`
- [x] 3.4 Run `dotnet build` and confirm no regressions

## 4. Update Example File

- [x] 4.1 Fetch the TOON spec (`https://github.com/0xZunia/ToonSharp/blob/main/SPEC.md`) to confirm valid TOON syntax
- [x] 4.2 Rewrite `examples/toon-data.toon` from YAML syntax to valid TOON syntax (at minimum: a few key-value entries and one array)
- [x] 4.3 Verify `ToonSerializer.serialize` round-trips `examples/toon-data.toon` without error (manually or via test)

## 5. Tests

- [x] 5.1 Add C-category test: `ToonSerializer.serialize` round-trips `examples/toon-data.toon` — assert `Ok` result and expected key present in output
- [x] 5.2 Add C-category test: `ToonSerializer.serialize` with malformed TOON string — assert `Error` result and no exception thrown
- [x] 5.3 Add C-category test: `resolveImportedContent` with `DataFormat.Yaml` — assert raw file content returned unchanged (regression)
- [x] 5.4 Extend existing A-category TOON acceptance test (`AgentWriterTests.fs:144–154`) to assert embedded content is valid TOON (key present after parse, not raw YAML)
- [x] 5.5 Run `dotnet test --filter "category=toon"` and confirm all new tests pass
- [x] 5.6 Run `dotnet test` and confirm all existing tests still pass

## 6. Changelog & Cleanup

- [x] 6.1 Add entry to `CHANGELOG.md` noting new `ToonSerializer` module, `resolveImportedContent` refactor, `ToonSharp` dependency, and `examples/toon-data.toon` fix
