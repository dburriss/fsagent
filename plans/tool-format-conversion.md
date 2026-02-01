# Plan: Support Both Tool Configuration Formats

## Executive Summary

**Problem**: Tool configuration format differs between agent systems:
- **Copilot/Claude**: List format `tools: ["read", "edit"]` (allowlist)
- **OpenCode**: Can use list OR map format `tools: { bash: false, edit: true }` (explicit enable/disable)

**Solution**: This is a **writer concern**, not a DSL concern. The Writers layer should convert tool configurations based on the target `OutputFormat`.

**Key architectural insight**: The DSL should store tools in a flexible intermediate format, and Writers should transform it to the target format (Opencode, Copilot, Claude) based on the output configuration.

---

# Plan: Support Both Tool Configuration Formats

## Problem Statement

The codebase currently supports only **one** of two tool configuration formats used in agent systems. We need to support both:

### Format 1: List-Based (Currently Supported)
Used by: Copilot, Claude, some OpenCode configurations

```yaml
tools:
  - grep
  - bash
  - read
```

**Current DSL**: `tools ["grep"; "bash"; "read"]`

### Format 2: Map-Based (NOT Currently Supported)
Used by: OpenCode for fine-grained enable/disable control

```yaml
tools:
  bash: false
  write: false
  edit: true
  webfetch: true
```

**Desired DSL**: Need to add support for this format

## Evidence from Documentation

### From `knowledge/agent-tools.md` (lines 13-20):
```markdown
Tools are specified in the YAML frontmatter of markdown agent files:

```yaml
tools:
  bash: false
  write: false
  edit: true
  webfetch: true
```

**Key insight**: Each tool has an explicit true/false value for enable/disable

### From `knowledge/agents.md` (lines 28-30, 82-84):
Shows both formats are valid:
- OpenCode example: list format
- Copilot example: list format
- But `agent-tools.md` shows OpenCode also uses map format!

### Available Tools (from `agent-tools.md`):
- `write` - File creation and writing
- `edit` - Code modification and patching
- `bash` - System command execution
- `webfetch` - Web content retrieval
- `todo` - Task management
- Wildcard patterns: `mymcp_*: false` for MCP server tools

## Revised Design: Writer-Based Conversion

### Architectural Approach

**Separation of Concerns**:
1. **DSL Layer**: Format-agnostic tool specification
2. **AST Layer**: Store tools in flexible format
3. **Writers Layer**: Convert to target format based on `OutputFormat`

### DSL Design (Two Options)

#### Option A: Single Unified DSL (Recommended)

Support both list and map in DSL, store as-is in AST:

```fsharp
// List format (simple allowlist)
agent {
    tools ["grep"; "bash"; "read"]
}
// AST: Frontmatter["tools"] = obj list

// Map format (explicit enable/disable)
agent {
    toolMap [
        ("bash", false)
        ("write", false)
        ("edit", true)
    ]
}
// AST: Frontmatter["tools"] = Map<string, obj>
```

#### Option B: List-Only DSL (Simpler)

Keep DSL simple with list format only, let writers handle conversion:

```fsharp
// DSL: Always use list
agent {
    tools ["grep"; "bash"; "read"]
}

// Writers convert based on OutputFormat:
// - Copilot/Claude: ["grep", "bash", "read"]
// - OpenCode (list mode): ["grep", "bash", "read"]
// - OpenCode (map mode): { grep: true, bash: true, read: true }
```

### Writer Conversion Logic

Add new `ToolFormat` option to control output:

```fsharp
type ToolFormat =
    | ToolsList        // ["tool1", "tool2"]
    | ToolsMap         // { tool1: true, tool2: false }
    | Auto             // Based on OutputFormat (default)

type Options = {
    // ... existing fields
    mutable ToolFormat: ToolFormat
}
```

**Conversion in Writers.fs**:

```fsharp
let formatTools (toolsValue: obj) (opts: Options) : string =
    let targetFormat =
        match opts.ToolFormat with
        | Auto ->
            match opts.OutputFormat with
            | Copilot -> ToolsList
            | Opencode -> ToolsMap  // or configurable
        | explicit -> explicit

    match toolsValue, targetFormat with
    // List stored, list output
    | :? (obj list) as tools, ToolsList ->
        tools |> List.map string |> String.concat "\n  - " |> sprintf "\n  - %s"

    // List stored, map output (convert to map with all true)
    | :? (obj list) as tools, ToolsMap ->
        tools
        |> List.map (fun t -> sprintf "  %s: true" (string t))
        |> String.concat "\n"
        |> sprintf "\n%s"

    // Map stored, list output (only enabled tools)
    | :? Map<string, obj> as toolMap, ToolsList ->
        toolMap
        |> Map.filter (fun _ v -> v :?> bool)
        |> Map.keys
        |> Seq.map string
        |> String.concat "\n  - "
        |> sprintf "\n  - %s"

    // Map stored, map output
    | :? Map<string, obj> as toolMap, ToolsMap ->
        toolMap
        |> Map.toSeq
        |> Seq.map (fun (k, v) -> sprintf "  %s: %s" k (string v))
        |> String.concat "\n"
        |> sprintf "\n%s"
```

## Recommended Implementation: Writer-Based Conversion

### Phase 1: Add ToolFormat Option (Minimal Change)

```fsharp
// Writers.fs - Add new type and option field
type ToolFormat =
    | ToolsList        // Output: ["tool1", "tool2"] or list format
    | ToolsMap         // Output: { tool1: true, tool2: false }
    | Auto             // Auto-detect based on OutputFormat

type Options = {
    // ... existing fields ...
    mutable ToolFormat: ToolFormat
}

// Update defaultOptions
let defaultOptions () = {
    // ... existing ...
    ToolFormat = Auto
}
```

### Phase 2: Update Frontmatter Serialization

Modify the frontmatter serialization in `writeMd` to handle tools specially:

```fsharp
// In writeMd, replace existing tools serialization with:
let formatToolsFrontmatter (key: string) (value: obj) (opts: Options) =
    if key = "tools" then
        // Determine target format
        let targetFormat =
            match opts.ToolFormat with
            | Auto ->
                match opts.OutputFormat with
                | Copilot -> ToolsList
                | Opencode -> ToolsList  // Default to list, user can override
            | explicit -> explicit

        // Convert based on storage format and target format
        match value, targetFormat with
        | :? (obj list) as tools, ToolsList ->
            // List → List (pass through)
            tools |> List.map string |> String.concat "\n  - " |> sprintf "tools:\n  - %s"

        | :? (obj list) as tools, ToolsMap ->
            // List → Map (all enabled)
            tools
            |> List.map (fun t -> sprintf "  %s: true" (string t))
            |> String.concat "\n"
            |> sprintf "tools:\n%s"

        | :? Map<string, obj> as toolMap, ToolsList ->
            // Map → List (only enabled)
            toolMap
            |> Map.filter (fun _ v -> v :?> bool)
            |> Map.keys
            |> Seq.map string
            |> String.concat "\n  - "
            |> sprintf "tools:\n  - %s"

        | :? Map<string, obj> as toolMap, ToolsMap ->
            // Map → Map (pass through)
            toolMap
            |> Map.toSeq
            |> Seq.map (fun (k, v) -> sprintf "  %s: %s" k (string v |> _.ToLower()))
            |> String.concat "\n"
            |> sprintf "tools:\n%s"

        | _ -> sprintf "%s: %O" key value  // Fallback
    else
        // Non-tools field, use existing logic
        formatRegularFrontmatter key value
```

## Files to Modify

### Primary Changes

1. **`/Users/devon.burriss/Documents/GitHub/fsagent/main/src/FsAgent/Writers.fs`**
   - Add `ToolFormat` type (ToolsList, ToolsMap, Auto)
   - Add `ToolFormat` field to `Options` type
   - Update `defaultOptions` to include `ToolFormat = Auto`
   - Add special handling for `tools` frontmatter key in `writeMd`
   - Implement conversion logic between list and map formats

### Optional: DSL Enhancement

2. **`/Users/devon.burriss/Documents/GitHub/fsagent/main/src/FsAgent/Agent.fs`** (Optional)
   - Add `toolMap` operation to AgentBuilder for map-based input
   - Keep existing `tools` operation for list-based input

### Testing

3. **`/Users/devon.burriss/Documents/GitHub/fsagent/main/tests/FsAgent.Tests/MarkdownWriterTests.fs`**
   - Test list → list conversion (existing, verify)
   - Test list → map conversion
   - Test map → list conversion (filter enabled only)
   - Test map → map conversion
   - Test `Auto` format selection based on OutputFormat

### Documentation

4. **`/Users/devon.burriss/Documents/GitHub/fsagent/main/README.md`**
   - Document `ToolFormat` option
   - Show examples of both input formats
   - Explain when each output format is used

## Implementation Steps

### Step 1: Add ToolFormat Infrastructure
1. Add `ToolFormat` type to Writers.fs
2. Add field to `Options` type
3. Update `defaultOptions`
4. Write tests for format conversion

### Step 2: Implement Conversion Logic
1. Extract current tools serialization code
2. Add format detection logic
3. Implement 4 conversion paths (list→list, list→map, map→list, map→map)
4. Integrate into `writeMd` frontmatter serialization

### Step 3: Test Thoroughly
1. Test with list input, list output (Copilot/Claude)
2. Test with list input, map output (OpenCode map mode)
3. Test with map input, list output (convert to allowlist)
4. Test with map input, map output (OpenCode map mode)
5. Test Auto format selection

### Step 4: Documentation and Examples
1. Update README with ToolFormat option
2. Add example showing OpenCode map format
3. Update CHANGELOG with new feature

## Verification Plan

After implementation, verify all conversion paths:

```fsharp
// Test 1: List input → List output (default for Copilot)
let agent1 = agent {
    tools ["grep"; "bash"]
}
let output1 = MarkdownWriter.writeAgent agent1 (fun opts ->
    opts.OutputFormat <- Copilot
    opts.ToolFormat <- Auto)
// Should contain: tools:\n  - grep\n  - bash

// Test 2: List input → Map output (OpenCode map mode)
let output2 = MarkdownWriter.writeAgent agent1 (fun opts ->
    opts.OutputFormat <- Opencode
    opts.ToolFormat <- ToolsMap)
// Should contain: tools:\n  grep: true\n  bash: true

// Test 3: Map input → List output (convert to allowlist)
let agent3 = agent {
    meta (meta {
        kvObj "tools" (Map.ofList [
            ("bash", false :> obj)
            ("edit", true :> obj)
            ("read", true :> obj)
        ])
    })
}
let output3 = MarkdownWriter.writeAgent agent3 (fun opts ->
    opts.ToolFormat <- ToolsList)
// Should contain: tools:\n  - edit\n  - read
// (bash excluded because it's false)

// Test 4: Map input → Map output (pass through)
let output4 = MarkdownWriter.writeAgent agent3 (fun opts ->
    opts.ToolFormat <- ToolsMap)
// Should contain: tools:\n  bash: false\n  edit: true\n  read: true

// Test 5: Auto format selection
let output5 = MarkdownWriter.writeAgent agent1 (fun opts ->
    opts.OutputFormat <- Copilot
    opts.ToolFormat <- Auto)  // Should choose ToolsList for Copilot
Assert.Contains("  - grep", output5)

let output6 = MarkdownWriter.writeAgent agent1 (fun opts ->
    opts.OutputFormat <- Opencode
    opts.ToolFormat <- Auto)  // Should choose ToolsList for Opencode (default)
Assert.Contains("  - grep", output6)
```

## Summary

This approach properly separates concerns:
- **DSL**: Format-agnostic (supports both list and map)
- **AST**: Stores tools as-is (list or map)
- **Writers**: Converts to target format based on OutputFormat and ToolFormat option

Benefits:
1. ✅ Supports all documented formats (Copilot list, OpenCode list, OpenCode map)
2. ✅ User controls output format via Writer options
3. ✅ Backward compatible (existing list-based code works unchanged)
4. ✅ Flexible (can convert between formats as needed)
5. ✅ Follows architecture (Writers handle format conversion, DSL stays agnostic)
