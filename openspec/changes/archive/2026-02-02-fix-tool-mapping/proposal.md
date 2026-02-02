## Why

The `Tool` type is incomplete and the mapping logic doesn't handle all harness tools correctly. Currently, FsAgent only supports 5 tools (Write, Edit, Bash, WebFetch, Todo) but agent harnesses provide 13+ tools. Additionally, some FsAgent tools should map to multiple harness tools (e.g., TodoWrite → TaskCreate + TaskUpdate for Claude), and duplicate mappings should be deduplicated.

## What Changes

- Add missing tool variants to the `Tool` discriminated union: `Read`, `Glob`, `List`, `LSP`, `Skill`, `TodoWrite`, `TodoRead`, `WebSearch`, `Question`
- Split existing `Todo` into `TodoWrite` and `TodoRead` for finer control
- Rename `Bash` to `Shell` to match FsAgent naming convention (keep `Bash` as alias for backward compatibility)
- Update `toolToString` function to handle one-to-many mappings (e.g., TodoWrite → ["TaskCreate", "TaskUpdate"] for Claude)
- Add deduplication logic to ensure each harness tool appears only once in the output
- Update tool documentation and examples to reflect the complete tool set

## Capabilities

### New Capabilities
- `tool-harness-mapping`: Complete mapping between FsAgent Tool type and all three harness tool names (Opencode, Claude, Copilot) with support for one-to-many mappings and deduplication

### Modified Capabilities
- `ast-nodes`: Add new tool variants to the Tool discriminated union (non-breaking - additive only)

## Impact

- **Breaking Changes**: None (additive changes only, existing Tool variants remain unchanged)
- **Affected Code**:
  - `src/FsAgent/AST.fs` - Tool type definition
  - `src/FsAgent/Writers.fs` - toolToString function and tool formatting logic
  - Documentation and examples using tools
- **Dependencies**: None
- **Migration**: Users with existing agents continue to work. New tool variants are opt-in.
