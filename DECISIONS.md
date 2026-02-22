# Decision Log

A record of significant technical decisions made in this project.
Each entry captures context, options considered, and the rationale for the choice made.

---

## DEC-001 — File I/O Abstraction Library

**Date:** 2026-02-22
**Status:** Decided

### Context

The `FileWriter` module currently calls `System.IO` statics directly (`File.WriteAllText`, `Directory.CreateDirectory`).
This makes the C-category tests in `FileWriterTests` hit the real file system, requiring temp directories and try/finally cleanup.
Abstracting file I/O would allow in-memory tests and push real I/O to the boundary.

Two candidate libraries were evaluated:

| | TestableIO.System.IO.Abstractions | Testably.Abstractions |
|---|---|---|
| NuGet | `TestableIO.System.IO.Abstractions` | `Testably.Abstractions` |
| `netstandard2.0` | Explicit target | Computed (compatible) |
| Mock library | `TestableIO.System.IO.Abstractions.TestingHelpers` | `Testably.Abstractions.Testing` |
| Mock fidelity | Basic; not validated against real FS | Test suite runs against both mock and real FS |
| Advanced features | No (FileSystemWatcher partial, no SafeFileHandles, no drive management) | Yes (FileSystemWatcher, SafeFileHandles, multiple drives, cross-platform simulation) |
| Extra abstractions | None | `ITimeSystem`, `IRandomSystem` |
| Active development | Maintenance only | Active |
| Same `IFileSystem` interface | Yes (shared) | Yes (shared) |
| NuGet downloads | ~48M | ~210K |
| Relationship | Original; active dev deferred to Testably | Successor; same maintainer |

Both share the same `IFileSystem` interface, so production code is unaffected by which testing library is chosen.

### Decision

**Use `Testably.Abstractions` in production and `Testably.Abstractions.Testing` for tests.**

### Rationale

- `Testably.Abstractions` is the actively developed successor recommended by the shared maintainer for new features.
- Mock fidelity is verified: the test suite runs against both the mock and the real file system, making behaviour consistent.
- `netstandard2.0` is compatible (computed), satisfying the target framework requirement for the published library.
- The fluent `MockFileSystem` API (`.Initialize().WithFile(...)`) is cleaner than the dictionary-based `MockFileData` approach.
- Switching later requires only test-project changes; production code remains on `IFileSystem` regardless.

---

## DEC-002 — AgentFileWriter Type for File I/O Injection

**Date:** 2026-02-22
**Status:** Decided

### Context

The `FileWriter` module exposes flat functions (`writeAgent`, `writeSkill`, `writeCommand`) that each require the caller to pass `harness`, `scope`, and a `configure` options callback. This produces noisy call sites and no natural injection point for `IFileSystem` (see DEC-001).

`NOTES.md` already flagged the need for an `AgentFileWriter` that composes rendering and writing. `ARCHITECTURE.md` notes a future composable writer hierarchy. The question was how to structure the type.

Options considered:

| Option | Description | Trade-off |
|---|---|---|
| Thread `IFileSystem` as parameter | Add `IFileSystem` to every module function | Noisy call sites; no grouping of config |
| Module-level `IFileSystem` ref | Mutable global or thread-local | Breaks purity; untestable |
| `AgentFileWriter` type | Record/class holding `IFileSystem`, `WriteScope`, optional `configure` | Clean call sites; DI-friendly; one config point |

### Decision

**Introduce `AgentFileWriter` as a type with `IFileSystem`, `WriteScope`, and optional `configure` set at construction.**

```fsharp
type AgentFileWriter(fileSystem: IFileSystem, scope: WriteScope, ?configure: AgentWriter.Options -> unit)
    member _.WriteAgent(agent: Agent, harness: AgentHarness) : string
    member _.WriteSkill(skill: Skill, harness: AgentHarness) : string
    member _.WriteCommand(cmd: SlashCommand, harness: AgentHarness, ?ns: string) : string
```

Existing module-level functions (`writeFile`, `writeAgent`, `writeSkill`, `writeCommand`) are kept and internally delegate to `AgentFileWriter(RealFileSystem(), scope)` — no breaking change for existing callers.

### Rationale

- **Injection point**: `IFileSystem` is injected once at construction, enabling in-memory testing without touching call sites.
- **Cleaner API**: Callers configure harness and scope once; write methods take only the artifact and harness.
- **Aligned with ARCHITECTURE.md**: The composable writer hierarchy section anticipates wrapping writers with configuration — this is a step in that direction.
- **Non-breaking**: Module shims preserve backward compatibility; `AgentFileWriter` is purely additive.
- **`configure` as constructor arg**: Applies consistently to all writes from the same instance; rare enough that it does not belong on every method signature.
