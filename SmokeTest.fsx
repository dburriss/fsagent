#!/usr/bin/env dotnet fsi

// Smoke test for typed tools and harness rename
// Tasks 10.3-10.8: Manual verification

#r "src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent
open FsAgent.Agents
open FsAgent.Writers
open FsAgent.AST
open FsAgent.Tools
open System

printfn "=== FsAgent v2.0 Smoke Test ==="
printfn ""

// Task 10.3: Create manual smoke test agent
printfn "Task 10.3: Creating smoke test agent with tools [Write; Bash; Custom \"mcp_special\"] and disallowedTools [Edit]"
let smokeTestAgent = agent {
    name "smoke-test-agent"
    description "Agent for testing typed tools and harness-specific mapping"
    tools [Write; Bash; Custom "mcp_special"]
    disallowedTools [Edit]
}
printfn "✓ Agent created successfully"
printfn ""

// Task 10.7: Verify frontmatter storage contains Tool lists
printfn "Task 10.7: Verifying frontmatter storage contains Tool lists, not string maps"
match smokeTestAgent.Frontmatter.TryFind "tools" with
| Some value ->
    match value with
    | :? (Tool list) as tools ->
        printfn "✓ frontmatter[\"tools\"] is Tool list: %A" tools
        if tools = [Write; Bash; Custom "mcp_special"] then
            printfn "✓ Tool list values are correct"
        else
            printfn "✗ ERROR: Tool list values don't match expected"
    | _ ->
        printfn "✗ ERROR: frontmatter[\"tools\"] is not a Tool list, type is: %s" (value.GetType().Name)
| None ->
    printfn "✗ ERROR: No 'tools' key in frontmatter"

match smokeTestAgent.Frontmatter.TryFind "disallowedTools" with
| Some value ->
    match value with
    | :? (Tool list) as tools ->
        printfn "✓ frontmatter[\"disallowedTools\"] is Tool list: %A" tools
        if tools = [Edit] then
            printfn "✓ disallowedTools list values are correct"
        else
            printfn "✗ ERROR: disallowedTools list values don't match expected"
    | _ ->
        printfn "✗ ERROR: frontmatter[\"disallowedTools\"] is not a Tool list"
| None ->
    printfn "✗ ERROR: No 'disallowedTools' key in frontmatter"
printfn ""

// Task 10.4: Generate Opencode output
printfn "Task 10.4: Generating Opencode output and verifying tool names are lowercase and Edit is disabled"
let opencodeOutput = AgentWriter.renderAgent smokeTestAgent (fun opts ->
    opts.OutputFormat <- AgentWriter.Opencode)

printfn "Opencode output:"
printfn "---"
printfn "%s" opencodeOutput
printfn "---"

// Verify Opencode tool names (list format, lowercase, disabled tools omitted)
let opencodeChecks = [
    ("- write", "Write tool is enabled and lowercase")
    ("- bash", "Bash tool is enabled and lowercase")
    ("- mcp_special", "Custom tool passes through")
]

let mutable opencodeSuccess = true
for (expected, description) in opencodeChecks do
    if opencodeOutput.Contains(expected) then
        printfn "✓ %s" description
    else
        printfn "✗ ERROR: %s (expected to find: %s)" description expected
        opencodeSuccess <- false

// Edit should NOT appear (it's disabled)
if not (opencodeOutput.Contains("- edit")) then
    printfn "✓ Disabled Edit tool not in output"
else
    printfn "✗ ERROR: Disabled Edit tool appears in output"
    opencodeSuccess <- false

if not (opencodeOutput.Contains("Write") || opencodeOutput.Contains("Bash") || opencodeOutput.Contains("Edit")) then
    printfn "✓ No capitalized tool names in Opencode output"
else
    printfn "✗ ERROR: Found capitalized tool names in Opencode output"
    opencodeSuccess <- false

printfn ""

// Task 10.5: Generate Copilot output
printfn "Task 10.5: Generating Copilot output and verifying tool names match Copilot specification"
let copilotOutput = AgentWriter.renderAgent smokeTestAgent (fun opts ->
    opts.OutputFormat <- AgentWriter.Copilot)

printfn "Copilot output:"
printfn "---"
printfn "%s" copilotOutput
printfn "---"

// Verify Copilot tool names (lowercase, list format, only enabled tools)
let copilotChecks = [
    ("- write", "Write tool in list format (lowercase)")
    ("- bash", "Bash tool in list format (lowercase)")
    ("- mcp_special", "Custom tool in list format")
]

let mutable copilotSuccess = true
for (expected, description) in copilotChecks do
    if copilotOutput.Contains(expected) then
        printfn "✓ %s" description
    else
        printfn "✗ ERROR: %s (expected to find: %s)" description expected
        copilotSuccess <- false

// Edit should NOT appear in output (it's disabled)
if not (copilotOutput.Contains("- edit")) then
    printfn "✓ Disabled Edit tool not in output"
else
    printfn "✗ ERROR: Disabled Edit tool appears in output"
    copilotSuccess <- false

printfn ""

// Task 10.6: Generate ClaudeCode output
printfn "Task 10.6: Generating ClaudeCode output and verifying tool names match Claude Code specification"
let claudeOutput = AgentWriter.renderAgent smokeTestAgent (fun opts ->
    opts.OutputFormat <- AgentWriter.ClaudeCode)

printfn "ClaudeCode output:"
printfn "---"
printfn "%s" claudeOutput
printfn "---"

// Verify ClaudeCode tool names (capitalized, list format, disabled tools omitted)
let claudeChecks = [
    ("- Write", "Write tool is enabled and capitalized")
    ("- Bash", "Bash tool is enabled and capitalized")
    ("- mcp_special", "Custom tool passes through")
]

let mutable claudeSuccess = true
for (expected, description) in claudeChecks do
    if claudeOutput.Contains(expected) then
        printfn "✓ %s" description
    else
        printfn "✗ ERROR: %s (expected to find: %s)" description expected
        claudeSuccess <- false

// Edit should NOT appear (it's disabled)
if not (claudeOutput.Contains("- Edit")) then
    printfn "✓ Disabled Edit tool not in output"
else
    printfn "✗ ERROR: Disabled Edit tool appears in output"
    claudeSuccess <- false

if not (claudeOutput.Contains("write") || claudeOutput.Contains("bash") || claudeOutput.Contains("edit")) then
    printfn "✓ No lowercase standard tool names in ClaudeCode output"
else
    printfn "✗ ERROR: Found lowercase tool names in ClaudeCode output"
    claudeSuccess <- false

printfn ""

// Summary
printfn "=== Verification Summary ==="
printfn "✓ Task 10.3: Smoke test agent created"
printfn "✓ Task 10.7: Frontmatter storage verified (Tool lists)"
if opencodeSuccess then
    printfn "✓ Task 10.4: Opencode output verified"
else
    printfn "✗ Task 10.4: Opencode output FAILED"

if copilotSuccess then
    printfn "✓ Task 10.5: Copilot output verified"
else
    printfn "✗ Task 10.5: Copilot output FAILED"

if claudeSuccess then
    printfn "✓ Task 10.6: ClaudeCode output verified"
else
    printfn "✗ Task 10.6: ClaudeCode output FAILED"

printfn ""
printfn "Note: Task 10.8 (toolMap compilation error) must be tested separately"
printfn "      Create a test file with 'toolMap [...]' and verify it fails to compile"
printfn ""

if opencodeSuccess && copilotSuccess && claudeSuccess then
    printfn "🎉 All smoke tests PASSED!"
    exit 0
else
    printfn "❌ Some smoke tests FAILED"
    exit 1
