## Context

FsAgent currently supports only 5 tools (Write, Edit, Bash, WebFetch, Todo) out of 13+ tools available across agent harnesses (Opencode, Claude Code, GitHub Copilot). The current `toolToString` function has a simple 1:1 mapping that doesn't handle cases where:
1. One FsAgent tool should map to multiple harness tools (e.g., TodoWrite → TaskCreate + TaskUpdate for Claude)
2. Multiple FsAgent tools map to the same harness tool and need deduplication
3. Different harnesses use different tool names for the same capability

The existing implementation in `Writers.fs` uses pattern matching on `(harness, tool)` pairs to return a single string. This needs to be extended to support one-to-many mappings.

## Goals / Non-Goals

**Goals:**
- Support all 13+ tools available across the three major agent harnesses
- Implement one-to-many tool mappings (one FsAgent tool → multiple harness tools)
- Deduplicate harness tools when multiple FsAgent tools map to the same underlying tool
- Maintain backward compatibility - existing agents continue to work unchanged
- Preserve type safety - all tools remain strongly typed with IDE autocomplete

**Non-Goals:**
- Support for dynamic/runtime tool registration
- Custom user-defined tool mapping overrides
- Tool capability validation or feature detection
- Changes to the `disallowedTools` logic (it already handles Tool lists correctly)

## Decisions

### Decision 1: Change toolToString signature from `string` to `string list`

**Rationale**:
- Enables one-to-many mappings (e.g., TodoWrite → ["TaskCreate", "TaskUpdate"])
- Simpler than creating a new function - just wrap existing single-tool cases in lists
- Allows deduplication at the call site using `List.distinct`

**Alternatives considered**:
- Keep `toolToString` returning `string` and create separate `toolToStrings` function → More complex API, harder to maintain
- Return `Set<string>` instead of `string list` → Over-engineered for the use case, list is sufficient
- Use active patterns to group tools → Adds complexity without clear benefit

**Implementation**:
```fsharp
let private toolToString (harness: AgentHarness) (tool: Tool) : string list =
    match harness, tool with
    | ClaudeCode, Tool.TodoWrite -> ["TaskCreate"; "TaskUpdate"]
    | ClaudeCode, Tool.TodoRead -> ["TaskList"; "TaskGet"; "TaskUpdate"]
    | Opencode, Tool.Shell -> ["bash"]
    // ... etc
```

### Decision 2: Add missing tool variants to Tool type (additive only)

**New tool variants**:
- `Read` - File reading capability
- `Glob` - File pattern matching
- `List` - Directory listing (maps to same as Glob for some harnesses)
- `LSP` - Language Server Protocol
- `Skill` - Execute predefined skills
- `TodoWrite` - Write todo items (replaces generic Todo)
- `TodoRead` - Read todo items (replaces generic Todo)
- `WebSearch` - Web search capability
- `Question` - Ask user questions
- `Shell` - Shell execution (rename of Bash, but keep Bash as alias)

**Rationale**:
- Covers all tools from the harness tools matrix
- Splitting Todo into TodoWrite/TodoRead allows finer control
- Keeping Bash as alias maintains backward compatibility

**Alternatives considered**:
- Remove old `Todo` and `Bash` variants → Breaking change, rejected
- Use a single `Todo` variant with write/read flag → Less type-safe, harder to use
- Keep `Bash` as primary name → Inconsistent with FsAgent naming (Shell is preferred)

### Decision 3: Deduplicate at formatting time, not at tool collection time

**Rationale**:
- Tools are collected as `Tool list`, deduplication happens when converting to strings
- Simpler logic - just `List.distinct` after mapping all tools
- Preserves user intent in the DSL (they specified TodoWrite and TodoRead, which both map to TaskUpdate for Claude)

**Implementation location**: In `formatToolsFrontmatter` function after calling `toolToString` for all tools:
```fsharp
let toolNames =
    tools
    |> List.collect (toolToString harness)
    |> List.distinct
```

**Alternatives considered**:
- Deduplicate at DSL level when tools are added → Wrong abstraction, harness-specific logic shouldn't leak to DSL
- Use Set instead of List → Doesn't preserve user ordering preference

### Decision 4: Keep backward compatibility for existing Todo and Bash

**Approach**:
- `Todo` maps to both TodoWrite and TodoRead capabilities (all harness tools)
- `Bash` is an alias for `Shell`
- Both are marked with `[<Obsolete>]` in a future version but work for now

**Rationale**:
- Zero breaking changes for existing users
- Clear migration path (Todo → TodoWrite/TodoRead, Bash → Shell)
- Obsolete warnings guide users to new API without forcing immediate changes

### Decision 5: Handle missing tools gracefully for harness-specific gaps

**Missing tool mappings** (from harness tools matrix):
- Opencode: `WebSearch` not available
- Copilot: `LSP` and `Question` not available

**Approach**:
- When a tool is not available for a harness, return empty list `[]` from `toolToString`
- Empty lists are filtered out during `List.collect`, so no tool is added to output
- Optionally log a warning when a tool is skipped (configurable via WriterOptions)

**Implementation**:
```fsharp
let private toolToString (harness: AgentHarness) (tool: Tool) : string list =
    match harness, tool with
    // Missing tools - return empty list
    | Opencode, Tool.WebSearch -> []
    | Copilot, Tool.LSP -> []
    | Copilot, Tool.Question -> []
    // ... normal mappings
    | Opencode, Tool.Shell -> ["bash"]
    | ClaudeCode, Tool.Shell -> ["Bash"]
```

**Rationale**:
- Users can write harness-agnostic agent definitions without worrying about tool availability
- Empty list naturally filters out during collection - no special handling needed
- Warning is optional so users aren't flooded with messages for intentional cross-harness agents

**Alternatives considered**:
- Throw exception for missing tools → Too strict, prevents cross-harness agent definitions
- Always warn about missing tools → Noisy for users who intentionally target multiple harnesses
- Add a "supported harnesses" property to Tool type → Over-engineered, mapping is sufficient

## Risks / Trade-offs

**[Risk]** One-to-many mappings could cause confusion when users see more harness tools than FsAgent tools they specified
→ **Mitigation**: Document the mapping behavior clearly, especially for TodoWrite/TodoRead

**[Risk]** Deduplication might hide user mistakes (e.g., specifying both Glob and List which map to same tool)
→ **Mitigation**: This is acceptable - it's a feature, not a bug. Users get what they want (the tool enabled) without errors

**[Risk]** Returning `string list` from `toolToString` is a breaking change for any code calling it
→ **Mitigation**: `toolToString` is private, so this is not a breaking change to the public API

**[Trade-off]** Keeping `Todo` and `Bash` for backward compatibility increases the size of the Tool DU
→ **Acceptable**: Two extra variants is minimal cost for avoiding breaking changes

**[Trade-off]** Claude's TodoRead mapping to TaskList + TaskGet + TaskUpdate feels "heavy"
→ **Acceptable**: Reflects Claude's actual API - all three tools work together for todo functionality

## Open Questions

None - design is ready for implementation.
