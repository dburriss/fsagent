#!/usr/bin/env dotnet fsi
#r "../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent.Agents
open FsAgent.Writers
open FsAgent.Tools

printfn "=== Typed Tools and Harness-Specific Mapping Examples ==="
printfn ""

// Example 1: Basic typed tools
printfn "1. Basic typed tools:"
printfn "---------------------"
let basicAgent = agent {
    name "basic-agent"
    description "Agent using typed tools"
    tools [Write; Edit; Read; Bash]
}
let basicOutput = MarkdownWriter.writeAgent basicAgent (fun _ -> ())
printfn "%s" basicOutput
printfn ""

// Example 2: Tools with disallowedTools (disabled tools omitted from output)
printfn "2. Tools with disallowedTools (disabled tools omitted):"
printfn "--------------------------------------------------------"
let filteredAgent = agent {
    name "filtered-agent"
    description "Agent with some tools disabled"
    tools [Write; Edit; Bash; Read; WebFetch]
    disallowedTools [Bash; Write]
}
let filteredOutput = MarkdownWriter.writeAgent filteredAgent (fun _ -> ())
printfn "%s" filteredOutput
printfn ""

// Example 3: Harness-specific tool names - Opencode (lowercase)
printfn "3. Opencode harness (lowercase tool names):"
printfn "--------------------------------------------"
let opencodeAgent = agent {
    name "opencode-agent"
    description "Agent for Opencode platform"
    tools [Write; Edit; Bash; Custom "mcp_database"]
}
let opencodeOutput = MarkdownWriter.writeAgent opencodeAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.Opencode)
printfn "%s" opencodeOutput
printfn ""

// Example 4: Harness-specific tool names - ClaudeCode (capitalized)
printfn "4. ClaudeCode harness (capitalized tool names):"
printfn "------------------------------------------------"
let claudeAgent = agent {
    name "claude-agent"
    description "Agent for Claude Code platform"
    tools [Write; Edit; Bash; Custom "mcp_database"]
}
let claudeOutput = MarkdownWriter.writeAgent claudeAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.ClaudeCode)
printfn "%s" claudeOutput
printfn ""

// Example 5: Same agent, different harnesses
printfn "5. Same agent definition, multiple harness outputs:"
printfn "----------------------------------------------------"
let multiHarnessAgent = agent {
    name "multi-harness-agent"
    description "Platform-agnostic agent definition"
    tools [Write; Bash; Read]
    disallowedTools [Edit]
}

printfn "Opencode output (lowercase):"
let multiOpencode = MarkdownWriter.writeAgent multiHarnessAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.Opencode)
printfn "%s" multiOpencode

printfn "ClaudeCode output (capitalized):"
let multiClaude = MarkdownWriter.writeAgent multiHarnessAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.ClaudeCode)
printfn "%s" multiClaude
printfn ""

// Example 6: Custom tools with MCP
printfn "6. Custom tools for MCP (Model Context Protocol):"
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

printfn "=== Summary ==="
printfn "✓ Type-safe tools: Write, Edit, Bash, Shell, Read, Glob, List, WebFetch, etc."
printfn "✓ IDE autocomplete for built-in tools"
printfn "✓ Harness-specific mapping:"
printfn "  - Opencode/Copilot: lowercase (write, edit, bash)"
printfn "  - ClaudeCode: capitalized (Write, Edit, Bash)"
printfn "✓ Custom tools pass through unchanged"
printfn "✓ Use 'tools' for enabled, 'disallowedTools' for disabled"
printfn "✓ Disabled tools are omitted from output"
printfn "✓ Write once, generate for any harness"
printfn ""
