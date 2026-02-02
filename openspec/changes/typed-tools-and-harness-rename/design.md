## Context

FsAgent currently uses stringly-typed tool configuration with `AgentFormat` as the platform identifier. The library has three tool-related operations (`tools`, `toolMap`, `disallowedTools`) with overlapping functionality. Tool names are converted to strings immediately at agent definition time, preventing platform-specific tool name mappings. The `AgentFormat` name is semantically unclear—it suggests file format rather than execution platform.

**Current Implementation:**
- `AgentFormat` DU with Opencode and Copilot cases
- Tools stored as `Map<string, obj>` in frontmatter at definition time
- Three operations: `tools` (obj list → all enabled), `toolMap` (explicit map), `disallowedTools` (merge with existing)
- No support for Claude Code platform
- No compile-time checking for tool names

**Constraints:**
- Must maintain single-file architecture (Library.fs acts as facade)
- Breaking changes are acceptable (major version bump)
- F# 2.0 compatibility required (netstandard2.0 target)
- xUnit test framework
- No backward compatibility required for this change

## Goals / Non-Goals

**Goals:**
- Provide type-safe tool configuration with IDE discoverability
- Support three agent platforms: Opencode, Copilot, ClaudeCode
- Enable platform-specific tool name mapping at write time
- Simplify API by removing redundant operations
- Improve semantic clarity with AgentHarness naming
- Maintain testability and code organization

**Non-Goals:**
- Supporting additional platforms beyond the three specified
- Backward compatibility with existing AgentFormat or string-based tools
- Runtime tool name validation (rely on compile-time safety)
- Tool capability detection or dynamic tool discovery
- Agent enrichment patterns (separate future work)

## Decisions

### Decision 1: Delayed Tool Resolution Architecture

**Choice:** Store Tool DU values in frontmatter; convert to strings at write time based on harness.

**Rationale:**
- Different harnesses use different tool names (e.g., ClaudeCode may capitalize: "Write" vs "write")
- Agent definition shouldn't know about target platform
- Enables single agent definition to target multiple platforms
- Writer has access to AgentHarness context for correct mapping

**Alternatives Considered:**
- **Convert at definition time:** Rejected because platform isn't known yet, and would require agents to be platform-specific
- **Store both Tool and string:** Rejected as redundant and error-prone
- **Add platform parameter to agent builder:** Rejected as premature—agents should be platform-agnostic

**Implementation:**
```fsharp
// Agent definition (platform-agnostic)
agent {
    tools [Write; Bash]
}
// Stores: frontmatter["tools"] = [Write; Bash] :> obj

// Writer (platform-specific)
writeMarkdown agent (fun opts -> opts.OutputFormat <- ClaudeCode)
// Converts: Write → "Write", Bash → "Bash" for ClaudeCode
```

### Decision 2: Separate tools and disallowedTools Storage

**Choice:** Store two separate Tool lists in frontmatter, merge in writer.

**Rationale:**
- Preserves user intent (enabled vs disabled)
- Allows writer to control merge strategy
- Simpler agent builder operations (no merge logic)
- Future extensibility for different merge strategies per platform

**Alternatives Considered:**
- **Merge at definition time:** Rejected because tool names aren't strings yet, complicates agent builder
- **Single (Tool * bool) list:** Rejected as less ergonomic than separate operations
- **Store final Map:** Rejected because can't delay string conversion

**Storage:**
```fsharp
frontmatter["tools"] = [Write; Edit; Bash] :> obj
frontmatter["disallowedTools"] = [Bash] :> obj
```

**Writer Merge:**
```fsharp
let enabledMap = tools |> List.map (fun t -> (toolToString harness t, true)) |> Map.ofList
let disabledMap = disallowed |> List.map (fun t -> (toolToString harness t, false)) |> Map.ofList
Map.fold (fun acc k v -> Map.add k v acc) enabledMap disabledMap  // disabled wins
```

### Decision 3: Tool DU with Limited Universal Set

**Choice:** Tool DU contains only universal tools (Write, Edit, Bash, WebFetch, Todo) plus Custom escape hatch.

**Rationale:**
- These five tools exist across all three platforms
- Custom(string) handles MCP tools and platform-specific tools
- Keeps DU manageable and focused
- No need to track every possible tool from every platform

**Alternatives Considered:**
- **All possible tools in DU:** Rejected as explosion of cases, many platform-specific
- **Only Custom(string):** Rejected as loses type safety for common cases
- **Separate DUs per platform:** Rejected as overly complex, agents should be portable

**Tool Type:**
```fsharp
type Tool =
    | Write
    | Edit
    | Bash
    | WebFetch
    | Todo
    | Custom of string
```

### Decision 4: Remove toolMap Operation

**Choice:** Delete toolMap entirely, users migrate to tools + disallowedTools.

**Rationale:**
- Redundant with tools + disallowedTools combination
- More verbose than alternatives
- API simplification reduces cognitive load
- Breaking change is acceptable for major version

**Migration Path:**
```fsharp
// Before: toolMap
agent {
    toolMap [("write", true); ("bash", false); ("edit", true)]
}

// After: tools + disallowedTools
agent {
    tools [Write; Edit]
    disallowedTools [Bash]
}
```

### Decision 5: AgentFormat → AgentHarness with ClaudeCode

**Choice:** Rename type and add ClaudeCode as third case.

**Rationale:**
- "Harness" better conveys execution platform semantics
- Distinct from OutputType (Md/Json/Yaml) which is serialization format
- ClaudeCode is a first-class platform alongside Opencode/Copilot
- Single rename opportunity for both changes (avoid multiple breaking changes)

**Type Definition:**
```fsharp
type AgentHarness =
    | Opencode
    | Copilot
    | ClaudeCode
```

### Decision 6: toolToString Function Location and Signature

**Choice:** Place in Writers.fs with signature `AgentHarness -> Tool -> string`.

**Rationale:**
- Writers.fs owns serialization logic
- Function needs access to both harness and tool for mapping
- Co-located with formatToolsFrontmatter that consumes it
- Not in AST.fs because Tool type shouldn't know about harnesses

**Implementation:**
```fsharp
let toolToString (harness: AgentHarness) (tool: Tool) : string =
    match harness, tool with
    | Opencode, Write -> "write"
    | Opencode, Edit -> "edit"
    | Copilot, Write -> "write"  // Same for now, may diverge
    | ClaudeCode, Write -> "Write"  // Capitalized (verify from docs)
    | _, Custom s -> s
```

**Note:** Actual tool names need verification from platform documentation. Start with placeholder mappings, refine during implementation.

### Decision 7: Tool Type Placement

**Choice:** Define Tool type in AST.fs alongside DataFormat.

**Rationale:**
- Fundamental type like DataFormat, Node
- Belongs with other domain types
- No dependencies on other modules
- Re-exported through Library.fs for public API

**File Organization:**
```fsharp
// AST.fs
type Tool = Write | Edit | Bash | WebFetch | Todo | Custom of string

// Library.fs
type Tool = AST.Tool  // Re-export for convenience
```

## Risks / Trade-offs

### [Risk] Breaking change for all existing users
**Mitigation:**
- Major version bump clearly signals breaking change
- Provide migration guide with before/after examples
- Document all signature changes in changelog
- Breaking changes are bundled (one migration, not multiple)

### [Risk] Tool name verification requires manual research
**Mitigation:**
- Start with placeholder mappings that work for Opencode
- Add TODO comments for Copilot and ClaudeCode names to verify
- Include links to documentation in comments
- Testing will catch incorrect names during smoke tests

### [Risk] Custom tool overuse reduces type safety benefits
**Mitigation:**
- Clear documentation on when to use Custom vs. universal tools
- Universal tools cover 90% of use cases
- Custom is necessary for MCP tools (no way to enumerate all)
- IDE autocomplete still guides users to universal tools first

### [Risk] Tool list extraction may fail if frontmatter format changes
**Mitigation:**
- Type-safe pattern matching in formatToolsFrontmatter
- Handle both `Tool list` and `list<obj>` for robustness
- Fail gracefully with empty list if type doesn't match
- Comprehensive tests for extraction logic

### [Risk] Separate storage increases frontmatter complexity
**Mitigation:**
- Writer handles merge transparently
- Agent authors don't need to understand internal storage
- Clear separation improves debuggability (can see enabled vs disabled)

### [Risk] Platform-specific tool names may diverge over time
**Mitigation:**
- Centralized toolToString function makes updates easy
- Pattern matching exhaustiveness ensures all harnesses handled
- Tests can validate mappings for each platform

## Migration Plan

### Phase 1: Core Types (No Breaking Impact Yet)
1. Add Tool type to AST.fs
2. Re-export Tool from Library.fs
3. Add AgentHarness type to Writers.fs (keep AgentFormat temporarily)
4. Add toolToString function to Writers.fs
5. Run tests to ensure no regressions

### Phase 2: Agent Builder Updates (Breaking)
6. Update tools operation signature: `obj list` → `Tool list`
7. Store Tool list directly in frontmatter (no string conversion)
8. Update disallowedTools operation signature: `string list` → `Tool list`
9. Store disallowed Tool list in separate frontmatter field
10. Delete toolMap operation completely

### Phase 3: Writer Updates (Breaking)
11. Replace AgentFormat with AgentHarness everywhere
12. Add ClaudeCode to all pattern matches
13. Update formatToolsFrontmatter to extract Tool lists
14. Implement tool-to-string conversion with harness context
15. Implement merge logic for enabled/disabled tools

### Phase 4: Test Updates
16. Update test syntax for typed tools
17. Remove toolMap test
18. Add tests for harness-specific tool name mapping
19. Update all AgentFormat references to AgentHarness
20. Add ClaudeCode test cases

### Phase 5: Documentation
21. Update CLAUDE.md with new types
22. Add migration guide with examples
23. Document breaking changes in changelog
24. Update API documentation

### Rollback Strategy
If critical issues discovered post-release:
- Revert entire commit (atomic change)
- Release hotfix with previous version
- Users can pin to previous version until migration ready
- No partial rollback needed (all changes are coupled)

### Verification
- `dotnet build` succeeds
- `dotnet test` all tests pass
- Manual smoke tests for each harness (Opencode, Copilot, ClaudeCode)
- Visual inspection of generated agent markdown for correct tool names

## Open Questions

### Q1: Exact tool names for Copilot and ClaudeCode?
**Status:** Needs research from documentation
- Opencode: Confirmed as lowercase (write, edit, bash, webfetch, todo)
- Copilot: TBD - check https://docs.github.com/en/copilot/reference/custom-agents-configuration#tools
- ClaudeCode: TBD - check https://code.claude.com/docs/en/settings#tools-available-to-claude

**Decision:** Implement with placeholders, refine during implementation phase.

### Q2: Should we validate tool names at write time?
**Status:** Deferred
- Pro: Catch invalid custom tool names early
- Con: Requires maintaining list of valid tools per platform
- Con: MCP tools are dynamic and can't be validated
**Decision:** Rely on compile-time checking for universal tools, no runtime validation for Custom.

### Q3: Future: Agent enrichment integration?
**Status:** Out of scope for this change
- See plans/agent-enricher.md for future design
- Enricher would work on Agent type after Tool resolution
- No impact on current design

## Implementation Files

**Modified Files:**
- `src/FsAgent/AST.fs` - Add Tool type
- `src/FsAgent/Library.fs` - Re-export Tool
- `src/FsAgent/Agent.fs` - Update tools/disallowedTools, remove toolMap
- `src/FsAgent/Writers.fs` - Rename AgentFormat → AgentHarness, add ClaudeCode, toolToString, update formatToolsFrontmatter
- `tests/FsAgent.Tests/MarkdownWriterTests.fs` - Update ~14 tests, remove 1

**No New Files Required:** All changes are modifications to existing files.
