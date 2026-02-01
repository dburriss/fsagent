# Tool Configuration Format Conversion

## Overview

FsAgent now supports both tool configuration formats used in agent systems:

1. **List Format (Allowlist)**: Used by Copilot, Claude, and OpenCode
2. **Map Format (Enable/Disable)**: Used by OpenCode for fine-grained control

The library can convert between these formats seamlessly based on your output requirements.

## Format Types

### List Format

An array of tool names representing enabled tools:

```yaml
tools:
  - grep
  - bash
  - read
```

**Use case**: Simple allowlist of available tools

### Map Format

A map of tool names to boolean values:

```yaml
tools:
  bash: false
  write: false
  edit: true
  webfetch: true
```

**Use case**: Explicit enable/disable control for each tool

## DSL Usage

### Creating List-Based Configuration

```fsharp
let agent = agent {
    name "my-agent"
    tools ["grep" :> obj; "bash" :> obj; "read" :> obj]
}
```

### Creating Map-Based Configuration

```fsharp
let agent = agent {
    name "my-agent"
    toolMap [
        ("bash", false)
        ("write", false)
        ("edit", true)
        ("read", true)
    ]
}
```

### Using Allowed and Disallowed Tools

Combine `tools` (allowed) with `disallowedTools` for convenience:

```fsharp
let agent = agent {
    name "my-agent"
    tools ["grep" :> obj; "bash" :> obj; "read" :> obj; "edit" :> obj]
    disallowedTools ["bash"; "write"]  // Disable specific tools
    // Result: grep, read, edit enabled; bash, write disabled
}
```

The `disallowedTools` operation:
- Creates a map-based configuration
- Merges with any existing `tools` or `toolMap` configuration
- Later operations override earlier ones (e.g., disallowedTools can disable a previously allowed tool)

## Writer Options

The `ToolFormat` option controls output format:

```fsharp
type ToolFormat =
    | ToolsList        // Output as array: ["tool1", "tool2"]
    | ToolsMap         // Output as map: { tool1: true, tool2: false }
    | Auto             // Based on OutputFormat (default)
```

### Examples

#### Default Behavior (Auto)

```fsharp
let output = MarkdownWriter.writeAgent agent (fun _ -> ())
// Auto selects ToolsList for both Copilot and Opencode
```

#### Force List Format

```fsharp
let output = MarkdownWriter.writeAgent agent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsList)
```

#### Force Map Format

```fsharp
let output = MarkdownWriter.writeAgent agent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
```

## Format Conversion

The writer automatically converts between formats:

### List → Map
All tools are set to `true`:

```fsharp
// Input: tools ["grep"; "bash"]
// Output with ToolsMap:
// tools:
//   grep: true
//   bash: true
```

### Map → List
Only enabled tools (value = `true`) are included:

```fsharp
// Input: toolMap [("bash", false); ("edit", true); ("read", true)]
// Output with ToolsList:
// tools:
//   - edit
//   - read
// (bash excluded because it's false)
```

### List → List
Pass through (no conversion):

```fsharp
// Input: tools ["grep"; "bash"]
// Output with ToolsList:
// tools:
//   - grep
//   - bash
```

### Map → Map
Pass through (no conversion):

```fsharp
// Input: toolMap [("bash", false); ("edit", true)]
// Output with ToolsMap:
// tools:
//   bash: false
//   edit: true
```

## Auto Format Selection

When `ToolFormat = Auto` (default), the format is selected based on `OutputFormat`:

| OutputFormat | Selected ToolFormat |
|--------------|---------------------|
| Copilot      | ToolsList           |
| Opencode     | ToolsList           |

You can override this behavior by explicitly setting `ToolFormat`.

## Complete Example

See `examples/tool-formats.fsx` for a working demonstration of all conversion scenarios.

## Design Rationale

This is a **writer concern**, not a DSL concern:

1. **DSL Layer**: Format-agnostic (supports both `tools` and `toolMap`)
2. **AST Layer**: Stores tools as-is (list or map)
3. **Writer Layer**: Converts to target format based on `OutputFormat` and `ToolFormat`

Benefits:
- ✅ Supports all documented formats (Copilot list, OpenCode list, OpenCode map)
- ✅ User controls output format via Writer options
- ✅ Backward compatible (existing list-based code works unchanged)
- ✅ Flexible (can convert between formats as needed)
- ✅ Follows architecture (Writers handle format conversion, DSL stays agnostic)

## Migration Guide

### From v0.2.x to v0.3.x

Existing code continues to work unchanged:

```fsharp
// Old code (still works)
let agent = agent {
    tools ["grep" :> obj; "bash" :> obj]
}
let output = MarkdownWriter.writeAgent agent (fun _ -> ())
```

New map-based configuration is optional:

```fsharp
// New capability (optional)
let agent = agent {
    toolMap [
        ("bash", false)
        ("edit", true)
    ]
}
let output = MarkdownWriter.writeAgent agent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
```

### Breaking Changes

None. This is a backward-compatible addition.

### New Features

1. `toolMap` DSL operation for map-based tool configuration
2. `ToolFormat` option in `Writers.Options`:
   - `Auto` (default): Based on `OutputFormat`
   - `ToolsList`: Force list format
   - `ToolsMap`: Force map format
3. Automatic conversion between list and map formats

## Testing

Run tests specific to tool format conversion:

```bash
dotnet test --filter "DisplayName~Tools"
dotnet test --filter "DisplayName~toolMap"
```

All format conversion paths are tested:
- List → List (pass through)
- List → Map (all enabled)
- Map → List (only enabled)
- Map → Map (pass through)
- Auto format selection
