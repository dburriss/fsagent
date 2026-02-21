## Context

`FsAgent` is a .NET Standard 2.0 library. All existing public functions (`renderAgent`, `renderSkill`, `renderCommand`, `renderPrompt`) return `string` and perform no I/O. Callers write the returned string to disk themselves, using whatever path they choose.

Each harness (Opencode, Copilot, ClaudeCode) has a fixed directory convention for agents, skills, and slash commands — at both project scope (relative to a repo root) and global scope (user config directory). These conventions are non-trivial and differ across harnesses. They are not currently encoded anywhere in the library.

The library already has the `AgentHarness` DU and the three artifact types (`Agent`, `Skill`, `SlashCommand`). The missing piece is a module that maps (harness × artifact type × scope × name) → output path, plus a thin I/O layer that creates directories and writes the file.

## Goals / Non-Goals

**Goals:**

- Add `resolveOutputPath` — pure function mapping harness + artifact kind + name + scope → absolute path string.
- Add `writeFile` — thin I/O wrapper: calls `resolveOutputPath`, creates intermediate directories, writes content, returns resolved path.
- Add convenience wrappers `writeAgent`, `writeSkill`, `writeCommand` that call the matching `render*` function then `writeFile`.
- Cover all three harnesses × three artifact types × two scopes (project, global).
- No breaking changes to existing API.
- No new NuGet dependencies.

**Non-Goals:**

- Deleting or overwrite-protecting existing files (callers decide; we always overwrite).
- Dry-run or preview mode.
- Reading/loading existing files from harness directories.
- Supporting harnesses beyond Opencode, Copilot, ClaudeCode.
- Generating multiple harness outputs in a single call (multi-harness write helper deferred).
- File watching or incremental output.

## Decisions

### D1: New file `FileWriter.fs`, not extending `Writers.fs`

**Decision:** Add `FileWriter.fs` as a separate source file, compiled after `Writers.fs`.

**Rationale:** `Writers.fs` is already 700+ lines. File I/O is a distinct concern; separating it keeps the pure render layer clean and the I/O layer independently testable. Follows the existing stratified design principle.

**Alternatives considered:**
- Append to `Writers.fs`: simpler at first, but violates the pure/IO boundary and makes the file unwieldy.
- Separate assembly: overkill for this scope.

---

### D2: `WriteScope` DU with `Project of string` and `Global`

**Decision:**
```fsharp
type WriteScope =
    | Project of rootDir: string
    | Global
```

**Rationale:** `Project` carries the root directory as data so the function is pure and testable without touching the filesystem. `Global` triggers OS-specific home directory resolution inside `resolveOutputPath`. Explicit DU is clearer than a `bool` flag or optional string.

**Alternatives considered:**
- `string option` for root: ambiguous (`None` = global? current dir?).
- Separate `resolveProjectPath` / `resolveGlobalPath` functions: more symbols, harder to compose.

---

### D3: `ArtifactKind` DU carries namespace for ClaudeCode commands

**Decision:**
```fsharp
type ArtifactKind =
    | AgentArtifact
    | CommandArtifact of namespace_: string option
    | SkillArtifact
```

**Rationale:** Only ClaudeCode commands support subdirectory namespacing (e.g. `.claude/commands/opsx/apply.md`). Rather than a separate parameter on every function, encoding it in the DU keeps the `resolveOutputPath` signature uniform.

**Alternatives considered:**
- Separate `namespace` parameter on `writeCommand`: leaks ClaudeCode-specific detail into every caller.
- Flat `CommandArtifact` with separate helper: requires callers to build the DU value before calling.

---

### D4: `resolveOutputPath` is pure; all I/O in `writeFile`

**Decision:** `resolveOutputPath` takes all inputs and returns a `string` path with no side effects. `writeFile` calls `resolveOutputPath` internally and performs `Directory.CreateDirectory` + `File.WriteAllText`.

**Rationale:** Pure path resolution is trivially unit-testable with no mocking. Acceptance tests can assert correct paths for every harness/kind/scope combination without touching disk.

---

### D5: Copilot `Global` scope raises `NotSupportedException`

**Decision:** Calling `resolveOutputPath` (or any `write*` convenience function) with `Copilot` harness and `Global` scope raises `NotSupportedException "Copilot does not support global file-system scope"`.

**Rationale:** Copilot global config is managed through GitHub org/enterprise settings, not the local filesystem. Silently ignoring the call or returning a nonsensical path would be worse than a clear exception.

**Alternatives considered:**
- Return `None` / `Result`: complicates the happy path for all callers just for one edge case.
- Write to `~/.github/` anyway: not a real Copilot convention; would be confusing.

---

### D6: Global path roots per harness and platform

| Harness | Linux/macOS | Windows |
|---|---|---|
| Opencode | `~/.config/opencode/` | `%APPDATA%\opencode\` |
| ClaudeCode | `~/.claude/` | `%USERPROFILE%\.claude\` |
| Copilot | — (not supported) | — |

Use `Environment.GetFolderPath(SpecialFolder.ApplicationData)` for Opencode on Windows and `Environment.GetFolderPath(SpecialFolder.UserProfile)` + `.config/opencode` on Unix. Detect Unix vs Windows via `Environment.OSVersion.Platform` or `RuntimeInformation.IsOSPlatform`.

---

### D7: Name extraction for `writeAgent`

Opencode agents use the **file name** as identity (no `name:` in frontmatter required). Copilot and ClaudeCode agents use `name:` from frontmatter. To keep the API uniform, `writeAgent` extracts name from `agent.Frontmatter["name"]` if present; if absent it raises `ArgumentException` with a message explaining that a `name` is needed to derive the output filename.

This means Opencode agent DSL users should include `name` in their `agent {}` block even though Opencode itself does not require it in the rendered file — or they must supply the name another way. A future overload `writeAgentAs name agent harness scope configure` can address this without changing the default.

## Risks / Trade-offs

**[Risk] Cross-platform path separator** → Mitigation: use `Path.Combine` throughout; never string-concatenate paths with `/`.

**[Risk] `rootDir` is relative, not absolute** → Mitigation: call `Path.GetFullPath rootDir` inside `resolveOutputPath` to normalise before combining; document that `Project "."` resolves to `Directory.GetCurrentDirectory()` at call time.

**[Risk] Copilot `.prompt.md` suffix vs plain `.md`** → The `.prompt.md` suffix is mandatory for Copilot VS Code prompt files. Encoding it in `resolveOutputPath` means callers don't need to know. Trade-off: slightly surprising if a caller inspects the returned path and sees a double extension.

**[Risk] Opencode `command/` vs `commands/` directory** → The canonical path is `.opencode/commands/`. The legacy `.opencode/command/` directory exists in this repo but is not used by the library. `resolveOutputPath` always produces `commands/` (plural).

**[Risk] Module is first to introduce file I/O** → All existing tests are in-memory. File writer tests that use `Project` scope will need a temp directory. `Global` scope tests can use `resolveOutputPath` (pure) to avoid touching the real config directory.

## Open Questions

1. **Opencode agent name**: Should there be a `writeAgentAs (name: string)` overload to allow explicit name without requiring `name:` in frontmatter? Deferred to follow-up if users report friction.
2. **Multi-harness convenience**: Should `writeAgentToAll` (writes to all three harnesses) be included in this change or deferred? Proposal excludes it — keep this change focused.
3. **Copilot Global scope**: Raise `NotSupportedException` (D5 above) or return `Result<string, string>` to avoid exceptions in the happy path? Currently leaning toward exception for consistency with `ValidationException` pattern already in the library.
