module MarkdownWriterTests

open Xunit
open FsAgent
open FsAgent.Agents
open FsAgent.Prompts
open FsAgent.Writers
open FsAgent.AST
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
let ``A: Tools map format outputs as list by default`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList ["tools", ([Custom "grep"; Bash; Custom "read"] :> obj)]
        Sections = []
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Contains("tools:", result)
    Assert.Contains("  - grep", result)
    Assert.Contains("  - bash", result)
    Assert.Contains("  - read", result)

[<Fact>]
let ``A: Tools map format outputs as map when ToolsMap specified`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList ["tools", ([Custom "grep"; Bash; Custom "read"] :> obj)]
        Sections = []
    }
    let result = MarkdownWriter.writeAgent agent (fun opts -> opts.ToolFormat <- MarkdownWriter.ToolsMap)
    Assert.Contains("tools:", result)
    Assert.Contains("  grep: true", result)
    Assert.Contains("  bash: true", result)
    Assert.Contains("  read: true", result)

[<Fact>]
let ``A: Tools map format with mixed boolean values outputs as map`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList [
            "tools", ([Edit; Custom "read"] :> obj)
            "disallowedTools", ([Bash] :> obj)
        ]
        Sections = []
    }
    let result = MarkdownWriter.writeAgent agent (fun opts -> opts.ToolFormat <- MarkdownWriter.ToolsMap)
    Assert.Contains("tools:", result)
    Assert.Contains("  bash: false", result)
    Assert.Contains("  edit: true", result)
    Assert.Contains("  read: true", result)

[<Fact>]
let ``A: Tools map format converts to list when ToolsList specified (only enabled)`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList [
            "tools", ([Edit; Custom "read"] :> obj)
            "disallowedTools", ([Bash] :> obj)
        ]
        Sections = []
    }
    let result = MarkdownWriter.writeAgent agent (fun opts -> opts.ToolFormat <- MarkdownWriter.ToolsList)
    Assert.Contains("tools:", result)
    Assert.Contains("  - edit", result)
    Assert.Contains("  - read", result)
    Assert.DoesNotContain("  - bash", result)  // bash is false, should be excluded

[<Fact>]
let ``A: Auto format uses list for Copilot`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList [
            "name", "TestAgent" :> obj
            "description", "Test desc" :> obj
            "tools", ([Custom "grep"; Bash] :> obj)
        ]
        Sections = []
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Copilot
        opts.ToolFormat <- MarkdownWriter.Auto)
    Assert.Contains("  - grep", result)
    Assert.Contains("  - bash", result)

[<Fact>]
let ``A: Auto format uses list for Opencode by default`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList ["tools", ([Custom "grep"; Bash] :> obj)]
        Sections = []
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Opencode
        opts.ToolFormat <- MarkdownWriter.Auto)
    Assert.Contains("  - grep", result)
    Assert.Contains("  - bash", result)

[<Fact>]
let ``A: tools DSL operation creates list format`` () =
    let agent = agent {
        tools [Custom "grep"; Bash; Custom "read"]
    }
    let result = MarkdownWriter.writeAgent agent (fun _ -> ())
    Assert.Contains("  - grep", result)
    Assert.Contains("  - bash", result)
    Assert.Contains("  - read", result)

[<Fact>]
let ``B: Default options include ToolFormat Auto`` () =
    let opts = MarkdownWriter.defaultOptions()
    Assert.Equal(MarkdownWriter.Auto, opts.ToolFormat)

[<Fact>]
let ``A: disallowedTools alone creates map with disabled tools`` () =
    let agent = agent {
        disallowedTools [Bash; Write]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts -> opts.ToolFormat <- MarkdownWriter.ToolsMap)
    Assert.Contains("  bash: false", result)
    Assert.Contains("  write: false", result)

[<Fact>]
let ``A: disallowedTools combined with tools creates merged map`` () =
    let agent = agent {
        tools [Custom "grep"; Custom "read"]
        disallowedTools [Bash; Write]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts -> opts.ToolFormat <- MarkdownWriter.ToolsMap)
    Assert.Contains("  grep: true", result)
    Assert.Contains("  read: true", result)
    Assert.Contains("  bash: false", result)
    Assert.Contains("  write: false", result)

[<Fact>]
let ``A: disallowedTools combined with tools using universal Tool types`` () =
    let agent = agent {
        tools [Edit; Custom "read"]
        disallowedTools [Bash; Write]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts -> opts.ToolFormat <- MarkdownWriter.ToolsMap)
    Assert.Contains("  edit: true", result)
    Assert.Contains("  read: true", result)
    Assert.Contains("  bash: false", result)
    Assert.Contains("  write: false", result)

[<Fact>]
let ``A: disallowedTools can override previously allowed tools`` () =
    let agent = agent {
        tools [Custom "grep"; Bash; Custom "read"]
        disallowedTools [Bash]  // Disable bash
    }
    let result = MarkdownWriter.writeAgent agent (fun opts -> opts.ToolFormat <- MarkdownWriter.ToolsMap)
    Assert.Contains("  grep: true", result)
    Assert.Contains("  read: true", result)
    Assert.Contains("  bash: false", result)  // Should be false, overriding the allowed list

[<Fact>]
let ``A: disallowedTools with ToolsList output shows only enabled tools`` () =
    let agent = agent {
        tools [Custom "grep"; Bash; Custom "read"]
        disallowedTools [Bash]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts -> opts.ToolFormat <- MarkdownWriter.ToolsList)
    Assert.Contains("  - grep", result)
    Assert.Contains("  - read", result)
    Assert.DoesNotContain("  - bash", result)  // bash is disabled, shouldn't appear in list
// B - Building Tests: Harness-specific tool name mapping

[<Fact>]
let ``B: Opencode harness uses lowercase tool names`` () =
    let agent = agent {
        tools [Write; Edit; Bash]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Opencode
        opts.ToolFormat <- MarkdownWriter.ToolsList)
    Assert.Contains("  - write", result)
    Assert.Contains("  - edit", result)
    Assert.Contains("  - bash", result)

[<Fact>]
let ``B: Copilot harness uses correct tool names`` () =
    let agent = agent {
        name "test-agent"
        description "Test agent for Copilot"
        tools [Write; Edit; Bash]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Copilot
        opts.ToolFormat <- MarkdownWriter.ToolsList)
    Assert.Contains("  - write", result)
    Assert.Contains("  - edit", result)
    Assert.Contains("  - bash", result)

[<Fact>]
let ``B: ClaudeCode harness uses capitalized tool names`` () =
    let agent = agent {
        tools [Write; Edit; Bash]
    }
    let result = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode
        opts.ToolFormat <- MarkdownWriter.ToolsList)
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
    // Test Opencode
    let opencodeResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Opencode
        opts.ToolFormat <- MarkdownWriter.ToolsList)
    Assert.Contains("  - mcp_special", opencodeResult)
    Assert.Contains("  - my_tool", opencodeResult)

    // Test Copilot
    let copilotResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Copilot
        opts.ToolFormat <- MarkdownWriter.ToolsList)
    Assert.Contains("  - mcp_special", copilotResult)
    Assert.Contains("  - my_tool", copilotResult)

    // Test ClaudeCode
    let claudeResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode
        opts.ToolFormat <- MarkdownWriter.ToolsList)
    Assert.Contains("  - mcp_special", claudeResult)
    Assert.Contains("  - my_tool", claudeResult)

[<Fact>]
let ``B: Same Tool list produces different strings for different harnesses`` () =
    let agent = agent {
        tools [Write; Bash]
    }
    
    let opencodeResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.Opencode
        opts.ToolFormat <- MarkdownWriter.ToolsList)
    
    let claudeResult = MarkdownWriter.writeAgent agent (fun opts ->
        opts.OutputFormat <- MarkdownWriter.ClaudeCode
        opts.ToolFormat <- MarkdownWriter.ToolsList)
    
    // Opencode uses lowercase
    Assert.Contains("  - write", opencodeResult)
    Assert.Contains("  - bash", opencodeResult)
    
    // ClaudeCode uses capitalized
    Assert.Contains("  - Write", claudeResult)
    Assert.Contains("  - Bash", claudeResult)
