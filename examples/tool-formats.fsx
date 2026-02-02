#!/usr/bin/env dotnet fsi
#r "../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent.Agents
open FsAgent.Writers
open FsAgent.AST  // For Tool type

printfn "=== Typed Tools and Harness-Specific Mapping Examples ==="
printfn ""

// Example 1: Basic typed tools with ToolsList output
printfn "1. Typed tools with ToolsList output (default):"
printfn "------------------------------------------------"
let listAgent = agent {
    name "list-agent"
    description "Agent using typed tools"
    tools [Write; Edit; Custom "grep"; Custom "read"]
}
let listOutput = MarkdownWriter.writeAgent listAgent (fun _ -> ())
printfn "%s" listOutput
printfn ""

// Example 2: Tools with disallowedTools (ToolsMap format)
printfn "2. Tools with disallowedTools (ToolsMap shows enabled/disabled):"
printfn "----------------------------------------------------------------"
let mapAgent = agent {
    name "map-agent"
    description "Agent with enabled and disabled tools"
    tools [Write; Edit; Bash; Custom "read"; WebFetch]
    disallowedTools [Bash; Write]
}
let mapOutput = MarkdownWriter.writeAgent mapAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
printfn "%s" mapOutput
printfn ""

// Example 3: ToolsList output from tools + disallowedTools (only enabled shown)
printfn "3. ToolsList output (only enabled tools shown):"
printfn "-----------------------------------------------"
let listFromMapOutput = MarkdownWriter.writeAgent mapAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsList)
printfn "%s" listFromMapOutput
printfn ""

// Example 4: Harness-specific tool names - Opencode (lowercase)
printfn "4. Opencode harness (lowercase tool names):"
printfn "-------------------------------------------"
let opencodeAgent = agent {
    name "opencode-agent"
    description "Agent for Opencode platform"
    tools [Write; Edit; Bash; Custom "mcp_database"]
}
let opencodeOutput = MarkdownWriter.writeAgent opencodeAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.Opencode
    opts.ToolFormat <- MarkdownWriter.ToolsList)
printfn "%s" opencodeOutput
printfn ""

// Example 5: Harness-specific tool names - ClaudeCode (capitalized)
printfn "5. ClaudeCode harness (capitalized tool names):"
printfn "-----------------------------------------------"
let claudeAgent = agent {
    name "claude-agent"
    description "Agent for Claude Code platform"
    tools [Write; Edit; Bash; Custom "mcp_database"]
}
let claudeOutput = MarkdownWriter.writeAgent claudeAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.ClaudeCode
    opts.ToolFormat <- MarkdownWriter.ToolsList)
printfn "%s" claudeOutput
printfn ""

// Example 6: Same agent, different harnesses
printfn "6. Same agent definition, multiple harness outputs:"
printfn "---------------------------------------------------"
let multiHarnessAgent = agent {
    name "multi-harness-agent"
    description "Platform-agnostic agent definition"
    tools [Write; Bash; Custom "github_api"]
    disallowedTools [Edit]
}

printfn "Opencode output (lowercase):"
let multiOpencode = MarkdownWriter.writeAgent multiHarnessAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.Opencode
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
printfn "%s" multiOpencode

printfn "ClaudeCode output (capitalized):"
let multiClaude = MarkdownWriter.writeAgent multiHarnessAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.ClaudeCode
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
printfn "%s" multiClaude
printfn ""

// Example 7: Copilot with Auto format selection
printfn "7. Copilot harness with Auto format (uses ToolsList):"
printfn "-----------------------------------------------------"
let copilotAgent = agent {
    name "copilot-agent"
    description "Agent for GitHub Copilot"
    tools [Write; Bash; WebFetch; Custom "search"]
    disallowedTools [Bash]
}
let copilotOutput = MarkdownWriter.writeAgent copilotAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.Copilot
    opts.ToolFormat <- MarkdownWriter.Auto)  // Auto selects ToolsList for Copilot
printfn "%s" copilotOutput
printfn ""

// Example 8: Custom tools with MCP
printfn "8. Custom tools for MCP (Model Context Protocol):"
printfn "--------------------------------------------------"
let mcpAgent = agent {
    name "mcp-claude-agent"
    description "Agent with MCP tools"
    tools [
        Write
        Bash
        Custom "mcp_database"
        Custom "mcp_filesystem"
        Custom "github_api"
    ]
}
let mcpOutput = MarkdownWriter.writeAgent mcpAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.ClaudeCode)
printfn "%s" mcpOutput
printfn ""

// Example 9: Only disallowedTools (no enabled tools)
printfn "9. Agent with only disallowedTools (all disabled):"
printfn "--------------------------------------------------"
let disabledOnlyAgent = agent {
    name "restricted-agent"
    description "Agent with all tools disabled"
    disallowedTools [Write; Edit; Bash]
}
let disabledOutput = MarkdownWriter.writeAgent disabledOnlyAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.ClaudeCode
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
printfn "%s" disabledOutput
printfn ""

printfn "=== Summary ==="
printfn "✓ Type-safe tools: Write, Edit, Bash, WebFetch, Todo, Custom \"name\""
printfn "✓ IDE autocomplete for built-in tools"
printfn "✓ Harness-specific mapping:"
printfn "  - Opencode/Copilot: lowercase (write, edit, bash)"
printfn "  - ClaudeCode: capitalized (Write, Edit, Bash)"
printfn "✓ Custom tools pass through unchanged"
printfn "✓ Use 'tools' for enabled, 'disallowedTools' for disabled"
printfn "✓ Output formats: ToolsList (array) or ToolsMap (boolean map)"
printfn "✓ Write once, generate for any harness"
printfn ""
