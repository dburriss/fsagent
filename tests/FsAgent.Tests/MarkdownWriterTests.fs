module MarkdownWriterTests

open Xunit
open FsAgent
open FsAgent.Agents
open FsAgent.Prompts
open FsAgent.Writers
open FsAgent.AST
open FsAgent.Tools
open System.IO

// A - Acceptance Tests: End-to-end DSL → AST → Writer pipeline

[<Fact>]
let ``A: Default writeMarkdown produces Markdown with ATX headings and frontmatter`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList ["description", "Test agent" :> obj]
        Sections = [
            Prompt.role "You are a test agent"
            Prompt.objective "Test objective"
        ]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    Assert.Contains("# role", result)
    Assert.Contains("# objective", result)
    Assert.Contains("---", result)
    Assert.Contains("description: Test agent", result)

[<Fact>]
let ``A: Copilot format includes name and description in frontmatter`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList ["name", "TestAgent" :> obj; "description", "Test desc" :> obj]
        Sections = [Prompt.role "Role"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts -> opts.OutputFormat <- MarkdownWriter.Copilot)
    Assert.Contains("name: TestAgent", result)
    Assert.Contains("description: Test desc", result)

[<Fact>]
let ``A: importRaw embeds file content without code fences`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "Imported content")
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Imported(tempFile, Yaml, false)]  // wrapInCodeBlock = false
    }
    let result = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    Assert.Contains("Imported content", result)
    Assert.DoesNotContain("```", result)
    File.Delete(tempFile)

[<Fact>]
let ``A: Heading rename map applies correctly`` () =
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Prompt.role "Role text"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts -> opts.RenameMap <- Map.ofList ["role", "Agent Role"])
    Assert.Contains("# Agent Role", result)

[<Fact>]
let ``A: Heading formatter applies after renames`` () =
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Prompt.role "Role text"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts ->
        opts.RenameMap <- Map.ofList ["role", "agent role"]
        opts.HeadingFormatter <- Some (fun s -> s.ToUpper()))
    Assert.Contains("# AGENT ROLE", result)

[<Fact>]
let ``A: Footer generator appends content`` () =
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Prompt.role "Role"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts ->
        opts.GeneratedFooter <- Some (fun ctx -> "Footer text"))
    Assert.Contains("Footer text", result)

[<Fact>]
let ``A: Output type JSON produces JSON`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList ["key", "value" :> obj]
        Sections = [Prompt.role "Role"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts -> opts.OutputType <- MarkdownWriter.Json)
    Assert.Contains("\"frontmatter\"", result)
    Assert.Contains("\"sections\"", result)

[<Fact>]
let ``A: Output type YAML produces YAML`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList ["key", "value" :> obj]
        Sections = [Prompt.role "Role"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts -> opts.OutputType <- MarkdownWriter.Yaml)
    Assert.Contains("frontmatter:", result)
    Assert.Contains("sections:", result)

[<Fact>]
let ``A: Custom writer overrides default`` () =
    let agent: Agent = { Frontmatter = Map.empty; Sections = [] }
    let result = MarkdownWriter.writeMarkdown agent (fun opts ->
        opts.CustomWriter <- Some (fun _ _ -> "Custom output"))
    Assert.Equal("Custom output", result)

[<Fact>]
let ``A: Deterministic output for same agent and options`` () =
    let agent: Agent = { Frontmatter = Map.empty; Sections = [Prompt.role "Role"] }
    let result1 = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    let result2 = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    Assert.Equal(result1, result2)

[<Fact>]
let ``A: import wraps JSON in json fence by default`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, """{"key": "value"}""")
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Imported(tempFile, Json, true)]  // wrapInCodeBlock = true
    }
    let result = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    Assert.Contains("```json", result)
    Assert.Contains("""{"key": "value"}""", result)
    File.Delete(tempFile)

[<Fact>]
let ``A: import wraps YAML in yaml fence by default`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "key: value")
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Imported(tempFile, Yaml, true)]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    Assert.Contains("```yaml", result)
    Assert.Contains("key: value", result)
    File.Delete(tempFile)

[<Fact>]
let ``A: import wraps TOON in toon fence by default`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "toon content here")
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Imported(tempFile, Toon, true)]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    Assert.Contains("```toon", result)
    Assert.Contains("toon content here", result)
    File.Delete(tempFile)

[<Fact>]
let ``A: import uses plain fence for Unknown format`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "unknown content")
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Imported(tempFile, Unknown, true)]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    Assert.Contains("```\n", result)
    Assert.Contains("unknown content", result)
    File.Delete(tempFile)

[<Fact>]
let ``A: DisableCodeBlockWrapping forces raw output even for import`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, """{"key": "value"}""")
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Imported(tempFile, Json, true)]  // wrapInCodeBlock = true
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts -> opts.DisableCodeBlockWrapping <- true)
    Assert.DoesNotContain("```json", result)
    Assert.Contains("""{"key": "value"}""", result)
    File.Delete(tempFile)

// C - Communication Tests: External boundaries

[<Fact>]
let ``C: Copilot format fails without name or description`` () =
    let agent: Agent = { Frontmatter = Map.empty; Sections = [] }
    Assert.Throws<System.Exception>(fun () ->
        MarkdownWriter.writeMarkdown agent (fun opts -> opts.OutputFormat <- MarkdownWriter.Copilot) |> ignore)

// B - Building Tests: Temporary scaffolding (can be removed later)

[<Fact>]
let ``B: Default options are set correctly`` () =
    let opts = MarkdownWriter.defaultOptions()
    Assert.Equal(MarkdownWriter.Opencode, opts.OutputFormat)
    Assert.Equal(MarkdownWriter.Md, opts.OutputType)
    Assert.False(opts.DisableCodeBlockWrapping)
    Assert.True(opts.IncludeFrontmatter)
    Assert.True(opts.RenameMap.IsEmpty)
    Assert.Equal(None, opts.HeadingFormatter)
    Assert.Equal(None, opts.GeneratedFooter)
    Assert.Equal(None, opts.CustomWriter)
    Assert.True(opts.TemplateVariables.IsEmpty)

[<Fact>]
let ``B: writeMarkdown is alias to writeAgent for backward compatibility`` () =
    let agent: Agent = { Frontmatter = Map.empty; Sections = [Section("test", [Text "content"])] }
    let result1 = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    let result2 = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Equal(result1, result2)

[<Fact>]
let ``A: Template node renders with variable substitution`` () =
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Template "Hello {{{name}}}, you are {{{age}}} years old"]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.TemplateVariables <- Map.ofList [("name", "Alice" :> obj); ("age", "30" :> obj)])
    Assert.Contains("Hello Alice, you are 30 years old", result)

[<Fact>]
let ``A: TemplateFile node renders from file with variables`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "Welcome {{{user}}} to {{{app}}}")
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [TemplateFile tempFile]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.TemplateVariables <- Map.ofList [("user", "Bob" :> obj); ("app", "TestApp" :> obj)])
    Assert.Contains("Welcome Bob to TestApp", result)
    File.Delete(tempFile)

[<Fact>]
let ``A: Template with no variables renders unchanged`` () =
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [Template "Hello world"]
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Contains("Hello world", result)

[<Fact>]
let ``C: TemplateFile with missing file returns error message`` () =
    let agent: Agent = {
        Frontmatter = Map.empty
        Sections = [TemplateFile "/nonexistent/file.txt"]
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Contains("[Template file not found:", result)

// Tool Format Conversion Tests

[<Fact>]
let ``A: Opencode outputs tools as struct format by default`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList ["tools", ([Custom "grep"; Bash; Custom "read"] :> obj)]
        Sections = []
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Contains("tools:", result)
    Assert.Contains("  grep: true", result)
    Assert.Contains("  bash: true", result)
    Assert.Contains("  read: true", result)


[<Fact>]
let ``A: Opencode tools DSL operation creates struct format`` () =
    let agent = agent {
        tools [Custom "grep"; Bash; Custom "read"]
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Contains("  grep: true", result)
    Assert.Contains("  bash: true", result)
    Assert.Contains("  read: true", result)


[<Fact>]
let ``A: Opencode disallowedTools shows disabled tools as false`` () =
    let agent = agent {
        disallowedTools [Bash; Write]
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    // Disabled tools should appear with false in struct format
    Assert.Contains("  bash: false", result)
    Assert.Contains("  write: false", result)

[<Fact>]
let ``A: Opencode disallowedTools combined with tools shows both enabled and disabled`` () =
    let agent = agent {
        tools [Custom "grep"; Custom "read"]
        disallowedTools [Bash; Write]
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Contains("  grep: true", result)
    Assert.Contains("  read: true", result)
    // Disabled tools should appear with false
    Assert.Contains("  bash: false", result)
    Assert.Contains("  write: false", result)

[<Fact>]
let ``A: disallowedTools combined with tools using universal Tool types`` () =
    let agent = agent {
        tools [Edit; Custom "read"]
        disallowedTools [Bash; Write]
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Contains("  edit: true", result)
    Assert.Contains("  read: true", result)
    // Disabled tools should appear with false
    Assert.Contains("  bash: false", result)
    Assert.Contains("  write: false", result)

[<Fact>]
let ``A: disallowedTools can override previously allowed tools`` () =
    let agent = agent {
        tools [Custom "grep"; Bash; Custom "read"]
        disallowedTools [Bash]  // Disable bash
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Contains("  grep: true", result)
    Assert.Contains("  read: true", result)
    // Bash should appear as disabled
    Assert.Contains("  bash: false", result)

[<Fact>]
let ``A: Opencode disallowedTools output shows all tools with status`` () =
    let agent = agent {
        tools [Custom "grep"; Bash; Custom "read"]
        disallowedTools [Bash]
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Contains("  grep: true", result)
    Assert.Contains("  read: true", result)
    Assert.Contains("  bash: false", result)  // bash is disabled, shown with false
// B - Building Tests: Harness-specific tool name mapping

[<Fact>]
let ``B: Opencode harness uses lowercase tool names`` () =
    let agent = agent {
        tools [Write; Edit; Bash]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Opencode)
    Assert.Contains("  write: true", result)
    Assert.Contains("  edit: true", result)
    Assert.Contains("  bash: true", result)

[<Fact>]
let ``B: Copilot harness uses correct tool names`` () =
    let agent = agent {
        name "test-agent"
        description "Test agent for Copilot"
        tools [Write; Edit; Bash]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Copilot)
    Assert.Contains("  - write", result)
    Assert.Contains("  - edit", result)
    Assert.Contains("  - bash", result)

[<Fact>]
let ``B: ClaudeCode harness uses capitalized tool names`` () =
    let agent = agent {
        tools [Write; Edit; Bash]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode)
    Assert.Contains("  - Write", result)
    Assert.Contains("  - Edit", result)
    Assert.Contains("  - Bash", result)

[<Fact>]
let ``B: Custom tools passthrough unchanged for all harnesses`` () =
    let agent = agent {
        name "test-agent"
        description "Test agent for custom tools"
        tools [Custom "mcp_special"; Custom "my_tool"]
    }
    // Test Opencode - struct format
    let opencodeResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Opencode)
    Assert.Contains("  mcp_special: true", opencodeResult)
    Assert.Contains("  my_tool: true", opencodeResult)

    // Test Copilot - list format
    let copilotResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Copilot)
    Assert.Contains("  - mcp_special", copilotResult)
    Assert.Contains("  - my_tool", copilotResult)

    // Test ClaudeCode - list format
    let claudeResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode)
    Assert.Contains("  - mcp_special", claudeResult)
    Assert.Contains("  - my_tool", claudeResult)

[<Fact>]
let ``B: Same Tool list produces different strings for different harnesses`` () =
    let agent = agent {
        tools [Write; Bash]
    }

    let opencodeResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Opencode)

    let claudeResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode)

    // Opencode uses lowercase struct format
    Assert.Contains("  write: true", opencodeResult)
    Assert.Contains("  bash: true", opencodeResult)

    // ClaudeCode uses capitalized list format
    Assert.Contains("  - Write", claudeResult)
    Assert.Contains("  - Bash", claudeResult)

// Tests for one-to-many tool mappings
[<Fact>]
let ``B: ClaudeCode TodoWrite maps to TaskCreate and TaskUpdate`` () =
    let agent = agent {
        tools [TodoWrite]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode)
    Assert.Contains("  - TaskCreate", result)
    Assert.Contains("  - TaskUpdate", result)

[<Fact>]
let ``B: ClaudeCode TodoRead maps to TaskList, TaskGet, and TaskUpdate`` () =
    let agent = agent {
        tools [TodoRead]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode)
    Assert.Contains("  - TaskList", result)
    Assert.Contains("  - TaskGet", result)
    Assert.Contains("  - TaskUpdate", result)

// Tests for tool deduplication
[<Fact>]
let ``B: Copilot deduplicates Glob and List both mapping to search`` () =
    let agent = agent {
        name "test-agent"
        description "Test agent"
        tools [Tool.Glob; Tool.List]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Copilot)
    // Count occurrences of "search"
    let searchCount =
        result.Split([|'\n'|])
        |> Array.filter (fun line -> line.Trim() = "- search")
        |> Array.length
    Assert.Equal(1, searchCount)

[<Fact>]
let ``B: ClaudeCode deduplicates Glob and List both mapping to Glob`` () =
    let agent = agent {
        tools [Tool.Glob; Tool.List]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode)
    // Count occurrences of "Glob"
    let globCount =
        result.Split([|'\n'|])
        |> Array.filter (fun line -> line.Trim() = "- Glob")
        |> Array.length
    Assert.Equal(1, globCount)

[<Fact>]
let ``B: ClaudeCode deduplicates TaskUpdate from TodoWrite and TodoRead`` () =
    let agent = agent {
        tools [TodoWrite; TodoRead]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode)
    // TaskUpdate should appear only once despite being in both mappings
    let taskUpdateCount =
        result.Split([|'\n'|])
        |> Array.filter (fun line -> line.Trim() = "- TaskUpdate")
        |> Array.length
    Assert.Equal(1, taskUpdateCount)
    // But all unique tools should be present
    Assert.Contains("  - TaskCreate", result)
    Assert.Contains("  - TaskList", result)
    Assert.Contains("  - TaskGet", result)

// Tests for missing tool handling
[<Fact>]
let ``B: Opencode omits WebSearch when not supported`` () =
    let agent = agent {
        tools [Write; WebSearch; Edit]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Opencode)
    Assert.Contains("  write: true", result)
    Assert.Contains("  edit: true", result)
    Assert.DoesNotContain("WebSearch", result)
    Assert.DoesNotContain("websearch", result)

[<Fact>]
let ``B: Copilot omits LSP when not supported`` () =
    let agent = agent {
        name "test-agent"
        description "Test agent"
        tools [Write; LSP; Edit]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Copilot)
    Assert.Contains("  - write", result)
    Assert.Contains("  - edit", result)
    Assert.DoesNotContain("LSP", result)
    Assert.DoesNotContain("lsp", result)

[<Fact>]
let ``B: Copilot omits Question when not supported`` () =
    let agent = agent {
        name "test-agent"
        description "Test agent"
        tools [Write; Question; Edit]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Copilot)
    Assert.Contains("  - write", result)
    Assert.Contains("  - edit", result)
    Assert.DoesNotContain("Question", result)
    Assert.DoesNotContain("question", result)

// Test for Custom tool pass-through across all harnesses
[<Fact>]
let ``B: Custom tools with special names pass through unchanged for all harnesses`` () =
    let agent = agent {
        name "test-agent"
        description "Test agent"
        tools [Custom "mcp_database"; Custom "github_api"; Custom "slack_api"]
    }

    // Test Opencode - struct format
    let opencodeResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Opencode)
    Assert.Contains("  mcp_database: true", opencodeResult)
    Assert.Contains("  github_api: true", opencodeResult)
    Assert.Contains("  slack_api: true", opencodeResult)

    // Test Copilot - list format
    let copilotResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Copilot)
    Assert.Contains("  - mcp_database", copilotResult)
    Assert.Contains("  - github_api", copilotResult)
    Assert.Contains("  - slack_api", copilotResult)

    // Test ClaudeCode - list format
    let claudeResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode)
    Assert.Contains("  - mcp_database", claudeResult)
    Assert.Contains("  - github_api", claudeResult)
    Assert.Contains("  - slack_api", claudeResult)

[<Fact>]
let ``B: Copilot uses list format not struct format`` () =
    let agent = agent {
        name "test-agent"
        description "Test agent"
        tools [Write; Edit]
        disallowedTools [Bash]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Copilot)
    Assert.Contains("  - write", result)
    Assert.Contains("  - edit", result)
    Assert.DoesNotContain("bash", result)  // Disabled tools omitted
    Assert.DoesNotContain(": true", result)  // No struct format

[<Fact>]
let ``B: ClaudeCode uses list format not struct format`` () =
    let agent = agent {
        tools [Write; Edit]
        disallowedTools [Bash]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode)
    Assert.Contains("  - Write", result)
    Assert.Contains("  - Edit", result)
    Assert.DoesNotContain("Bash", result)
    Assert.DoesNotContain(": true", result)

[<Fact>]
let ``A: Opencode struct format includes both enabled and disabled tools`` () =
    let agent = agent {
        tools [Write; Edit; Read]
        disallowedTools [Bash; WebFetch]
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    // Enabled tools
    Assert.Contains("  write: true", result)
    Assert.Contains("  edit: true", result)
    Assert.Contains("  read: true", result)
    // Disabled tools
    Assert.Contains("  bash: false", result)
    Assert.Contains("  webfetch: false", result)

[<Fact>]
let ``A: Opencode struct format is alphabetically sorted`` () =
    let agent = agent {
        tools [Write; Bash; Edit; Read]
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    let lines = result.Split('\n') |> Array.filter (fun l -> l.Contains(": true") || l.Contains(": false"))
    let toolOrder = lines |> Array.map (fun l -> l.Trim().Split(':').[0])
    Assert.Equal(4, toolOrder.Length)
    Assert.Equal("bash", toolOrder.[0])
    Assert.Equal("edit", toolOrder.[1])
    Assert.Equal("read", toolOrder.[2])
    Assert.Equal("write", toolOrder.[3])

[<Fact>]
let ``A: Empty tools for all harnesses`` () =
    let agentOpencode = agent { name "test"; description "test" }
    let agentCopilot = agent { name "test"; description "test" }
    let agentClaude = agent { name "test"; description "test" }

    let opencodeResult = MarkdownWriter.writeAgent agentOpencode (fun _ -> ())
    let copilotResult = MarkdownWriter.writeAgent agentCopilot (fun opts -> opts.OutputFormat <- MarkdownWriter.Copilot)
    let claudeResult = MarkdownWriter.writeAgent agentClaude (fun opts -> opts.OutputFormat <- MarkdownWriter.ClaudeCode)

    Assert.DoesNotContain("tools:", opencodeResult)
    Assert.DoesNotContain("tools:", copilotResult)
    Assert.DoesNotContain("tools:", claudeResult)

[<Fact>]
let ``A: Only disallowedTools for Copilot shows no tools section`` () =
    let agent = agent {
        name "test"
        description "test"
        disallowedTools [Bash; Write; Edit]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts -> opts.OutputFormat <- MarkdownWriter.Copilot)
    Assert.DoesNotContain("tools:", result)

[<Fact>]
let ``A: Only disallowedTools for ClaudeCode shows no tools section`` () =
    let agent = agent {
        disallowedTools [Bash; Write; Edit]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts -> opts.OutputFormat <- MarkdownWriter.ClaudeCode)
    Assert.DoesNotContain("tools:", result)
