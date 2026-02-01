#!/usr/bin/env dotnet fsi
#r "../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent.Agents
open FsAgent.Writers

printfn "=== Tool Configuration Format Examples ==="
printfn ""

// Example 1: List format (default)
printfn "1. List format (allowlist) - Default behavior:"
printfn "------------------------------------------------"
let listAgent = agent {
    name "list-agent"
    description "Agent using list format"
    tools ["grep" :> obj; "bash" :> obj; "read" :> obj]
}
let listOutput = MarkdownWriter.writeAgent listAgent (fun _ -> ())
printfn "%s" listOutput
printfn ""

// Example 2: Map format using toolMap DSL
printfn "2. Map format (enable/disable) - Using toolMap DSL:"
printfn "----------------------------------------------------"
let mapAgent = agent {
    name "map-agent"
    description "Agent using map format"
    toolMap [
        ("bash", false)
        ("write", false)
        ("edit", true)
        ("read", true)
        ("webfetch", true)
    ]
}
let mapOutput = MarkdownWriter.writeAgent mapAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
printfn "%s" mapOutput
printfn ""

// Example 3: Convert list to map
printfn "3. Convert list format to map format:"
printfn "--------------------------------------"
let convertedToMap = MarkdownWriter.writeAgent listAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
printfn "%s" convertedToMap
printfn ""

// Example 4: Convert map to list (only enabled tools)
printfn "4. Convert map format to list format (only enabled):"
printfn "-----------------------------------------------------"
let convertedToList = MarkdownWriter.writeAgent mapAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsList)
printfn "%s" convertedToList
printfn ""

// Example 5: Auto format selection
printfn "5. Auto format selection (Copilot uses list):"
printfn "----------------------------------------------"
let copilotAgent = agent {
    name "copilot-agent"
    description "Agent for Copilot"
    tools ["grep"; "bash"]
}
let copilotOutput = MarkdownWriter.writeAgent copilotAgent (fun opts ->
    opts.OutputFormat <- MarkdownWriter.Copilot
    opts.ToolFormat <- MarkdownWriter.Auto)  // Auto selects ToolsList for Copilot
printfn "%s" copilotOutput
printfn ""

// Example 6: Allow/Disallow pattern
printfn "6. Allow/Disallow pattern (combined):"
printfn "--------------------------------------"
let allowDisallowAgent = agent {
    name "mixed-agent"
    description "Agent using allow/disallow"
    tools ["grep" :> obj; "bash" :> obj; "read" :> obj; "edit" :> obj]
    disallowedTools ["bash"; "write"]  // Disable bash (override) and write
}
let allowDisallowOutput = MarkdownWriter.writeAgent allowDisallowAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsMap)
printfn "%s" allowDisallowOutput
printfn ""

// Example 7: disallowedTools converts to list (only enabled shown)
printfn "7. Allow/Disallow as list (only enabled shown):"
printfn "------------------------------------------------"
let allowDisallowListOutput = MarkdownWriter.writeAgent allowDisallowAgent (fun opts ->
    opts.ToolFormat <- MarkdownWriter.ToolsList)
printfn "%s" allowDisallowListOutput
printfn ""

printfn "=== Summary ==="
printfn "- List format: tools: [\"tool1\", \"tool2\"] - Allowlist of enabled tools"
printfn "- Map format: tools: { tool1: true, tool2: false } - Fine-grained enable/disable"
printfn "- Use 'tools' DSL for list, 'toolMap' DSL for map, 'disallowedTools' to disable specific tools"
printfn "- 'disallowedTools' can be combined with 'tools' for allow/deny pattern"
printfn "- Writer can convert between formats using ToolFormat option"
printfn "- Auto format selection based on OutputFormat (Copilot/Opencode)"
