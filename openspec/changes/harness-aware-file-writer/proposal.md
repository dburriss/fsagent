## Why

Users currently call `renderAgent`/`renderSkill`/`renderCommand` and get back a string, then must manually determine the correct output path for their target harness. This is error-prone and requires knowledge of per-harness directory conventions that the library already encodes internally.

## What Changes

- Add a new `FileWriter` module (or extend `AgentWriter`) with functions that write rendered content directly to disk at the correct harness-specific path.
- Functions must accept the harness (`AgentHarness`) and the artifact type (agent, skill, slash command) to determine the output path.
- Caller specifies scope: **project** (relative to a given root directory) or **global** (user home-relative or OS config path).
- Functions return the resolved output path so callers know where the file landed.
- File name / directory is derived from the artifact's `name` field; skills use `<name>/SKILL.md` layout.
- Copilot slash commands receive the `.prompt.md` suffix automatically.
- ClaudeCode commands support an optional namespace subdirectory.

## Capabilities

### New Capabilities

- `harness-aware-file-writer`: Write rendered agent/skill/command content to the correct harness-specific path without the caller knowing directory conventions. Covers project-scope and global-scope writes for all three harnesses (Opencode, Copilot, ClaudeCode).

### Modified Capabilities

- `markdown-writer`: No requirement changes — existing `render*` functions remain unchanged. The file-writer is an additive layer on top.

## Impact

- **New module**: `FsAgent.Writers.FileWriter` (or `FsAgent.Writers.AgentWriter` extension) — no breaking changes to existing API.
- **File I/O boundary**: This is the first module in `FsAgent` that performs file writes; I/O is pushed to a thin adapter layer, keeping render functions pure.
- **Global path resolution**: Requires `System.Environment.GetFolderPath` or equivalent for cross-platform home directory; no new NuGet dependencies.
- **Affected code**: `Writers.fs` (or a new `FileWriter.fs`), `Library.fs` (re-exports), and example scripts.
