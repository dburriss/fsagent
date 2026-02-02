# FsAgent Examples

This directory contains example scripts demonstrating FsAgent features with the new typed tools system.

## Running Examples

All examples are F# scripts (.fsx) that can be run with:

```bash
# From the repository root
dotnet fsi examples/script-name.fsx
```

## Examples

### tool-formats.fsx

Comprehensive demonstration of typed tools and harness-specific mapping.

**Features demonstrated:**
- Type-safe tool configuration with `Write`, `Edit`, `Bash`, `WebFetch`, `Todo`
- Custom tools for MCP and platform-specific tools: `Custom "tool_name"`
- Harness-specific tool name mapping (Opencode, Copilot, ClaudeCode)
- Output formats: ToolsList (array) and ToolsMap (boolean map)
- Tools with disallowedTools combinations
- Platform-agnostic agent definitions

**Key examples:**
1. Basic typed tools with ToolsList output
2. Tools with disallowedTools showing ToolsMap format
3. ToolsList filtering (only enabled tools)
4. Opencode harness (lowercase: write, edit, bash)
5. ClaudeCode harness (capitalized: Write, Edit, Bash)
6. Same agent, multiple harness outputs
7. Copilot with Auto format selection
8. Custom MCP tools
9. Agent with only disallowedTools

**Run it:**
```bash
dotnet fsi examples/tool-formats.fsx
```

### toon.fsx

Complete agent definition with prompts, metadata, and tools.

**Features demonstrated:**
- Reusable prompts with `prompt { ... }`
- Metadata configuration with `meta { ... }`
- Typed tools configuration
- File imports with relative paths
- Custom footer generation
- Output to markdown file

**Tools used:**
- `Custom "read"` - Custom read tool
- `Custom "search"` - Custom search tool
- `Edit` - Built-in edit tool (maps to "edit" for Opencode)

**Run it:**
```bash
dotnet fsi examples/toon.fsx
# Generates: examples/toon-agent.md
```

## Typed Tools Quick Reference

### Built-in Tools
```fsharp
open FsAgent.AST

tools [
    Write      // File writing
    Edit       // File editing
    Bash       // Shell commands
    WebFetch   // HTTP fetching
    Todo       // Task management
]
```

### Custom Tools
```fsharp
tools [
    Custom "mcp_database"     // MCP tool
    Custom "github_api"       // Platform-specific
    Custom "my_custom_tool"   // Any custom tool
]
```

### Disabling Tools
```fsharp
agent {
    tools [Write; Edit; Bash]
    disallowedTools [Bash]  // Bash will be disabled
}
```

## Harness-Specific Mapping

The same agent definition generates platform-specific output:

| Tool | Opencode | Copilot | ClaudeCode |
|------|----------|---------|------------|
| Write | write | write | Write |
| Edit | edit | edit | Edit |
| Bash | bash | bash | Bash |
| WebFetch | webfetch | webfetch | WebFetch |
| Todo | todo | todo | Todo |
| Custom "x" | x | x | x |

### Example
```fsharp
let agent = agent {
    name "my-agent"
    description "Platform-agnostic agent"
    tools [Write; Bash]
}

// Generate for Opencode (lowercase)
let opencode = MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.Opencode)
// Output: tools: [write, bash]

// Generate for ClaudeCode (capitalized)
let claude = MarkdownWriter.writeAgent agent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.ClaudeCode)
// Output: tools: [Write, Bash]
```

## Output Formats

### ToolsList (Array)
Shows only enabled tools:
```yaml
tools:
  - write
  - edit
  - bash
```

### ToolsMap (Boolean Map)
Shows all tools with enabled/disabled state:
```yaml
tools:
  write: true
  edit: true
  bash: false
```

### Auto
Automatically selects format based on harness (defaults to ToolsList).

## Migration from v1.x

**Before (v1.x):**
```fsharp
tools ["write" :> obj; "bash" :> obj]
disallowedTools ["bash"]
toolMap [("write", true); ("bash", false)]  // Removed in v2.0
```

**After (v2.0):**
```fsharp
open FsAgent.AST

tools [Write; Bash]
disallowedTools [Bash]
// toolMap is removed - use tools + disallowedTools instead
```

## More Information

- [CLAUDE.md](../CLAUDE.md) - Project architecture and API reference
- [MIGRATION.md](../MIGRATION.md) - Complete migration guide from v1.x
- [CHANGELOG.md](../CHANGELOG.md) - Version history and breaking changes
