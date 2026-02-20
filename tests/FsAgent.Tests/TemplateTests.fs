module TemplateTests

open Xunit
open FsAgent.Writers
open FsAgent.Tools
open System.IO

// Helper: minimal toolToString for test use
let private testToolToString (harness: AgentHarness) (tool: Tool) : string list =
    match harness, tool with
    | Opencode, Tool.Bash -> ["bash"]
    | Opencode, Tool.Read -> ["read"]
    | Copilot,  Tool.Bash -> ["bash"]
    | Copilot,  Tool.Read -> ["read"]
    | ClaudeCode, Tool.Bash -> ["Bash"]
    | ClaudeCode, Tool.Read -> ["Read"]
    | _, Tool.Custom s -> [s]
    | _ -> []

// Helper: minimal toolNameMap for test use
let private testToolNameMap : Map<string, Tool> =
    Map.ofList [
        "Bash", Tool.Bash
        "Read", Tool.Read
    ]

[<Fact>]
let ``Template.renderInline with variable substitution`` () =
    let variables = Map.ofList [("name", "Alice" :> obj); ("age", "30" :> obj)]
    let result = Template.renderInline "Hello {{{name}}}, age {{{age}}}" variables
    Assert.Equal("Hello Alice, age 30", result)

[<Fact>]
let ``Template.renderInline with no variables returns text unchanged`` () =
    let variables = Map.empty
    let result = Template.renderInline "Hello world" variables
    Assert.Equal("Hello world", result)

[<Fact>]
let ``Template.renderInline with missing variable leaves placeholder`` () =
    let variables = Map.ofList [("name", "Bob" :> obj)]
    let result = Template.renderInline "Hello {{{name}}}, age {{{age}}}" variables
    // With simple string replace, missing variables remain as-is
    Assert.Contains("Bob", result)

[<Fact>]
let ``Template.renderFile loads and renders file`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "Welcome {{{user}}} to {{{app}}}")
    let variables = Map.ofList [("user", "Charlie" :> obj); ("app", "TestApp" :> obj)]
    let result = Template.renderFile tempFile variables
    Assert.Equal("Welcome Charlie to TestApp", result)
    File.Delete(tempFile)

[<Fact>]
let ``Template.renderFile with missing file returns error message`` () =
    let result = Template.renderFile "/nonexistent/file.txt" Map.empty
    Assert.Contains("[Template file not found:", result)
    Assert.Contains("/nonexistent/file.txt", result)

[<Fact>]
let ``Template.renderFile with empty variables`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "Static content only")
    let result = Template.renderFile tempFile Map.empty
    Assert.Equal("Static content only", result)
    File.Delete(tempFile)

[<Fact>]
let ``Template.renderInline handles special characters in text`` () =
    let variables = Map.ofList [("var", "value" :> obj)]
    let result = Template.renderInline "Text with {{{var}}} and & < > symbols" variables
    Assert.Contains("value", result)
    Assert.Contains("&", result)

[<Fact>]
let ``Template.renderInline with numeric variables`` () =
    let variables = Map.ofList [("count", 42 :> obj); ("price", 99.99 :> obj)]
    let result = Template.renderInline "Count: {{{count}}}, Price: {{{price}}}" variables
    Assert.Contains("42", result)
    Assert.Contains("99.99", result)

// 4.1 – renderWithHarness resolves {{{tool Bash}}} for Opencode
[<Fact>]
let ``A: renderWithHarness resolves tool Bash for Opencode harness`` () =
    let result =
        Template.renderWithHarness
            "Use {{{tool Bash}}}"
            testToolNameMap
            testToolToString
            Opencode
            Map.empty
    Assert.Equal("Use bash", result)

// 4.2 – renderWithHarness resolves {{{tool Read}}} for Copilot
[<Fact>]
let ``A: renderWithHarness resolves tool Read for Copilot harness`` () =
    let result =
        Template.renderWithHarness
            "Use {{{tool Read}}}"
            testToolNameMap
            testToolToString
            Copilot
            Map.empty
    Assert.Equal("Use read", result)

// 4.3 – renderWithHarness resolves {{{tool Bash}}} for ClaudeCode
[<Fact>]
let ``A: renderWithHarness resolves tool Bash for ClaudeCode harness`` () =
    let result =
        Template.renderWithHarness
            "Use {{{tool Bash}}}"
            testToolNameMap
            testToolToString
            ClaudeCode
            Map.empty
    Assert.Equal("Use Bash", result)

// 4.4 – renderWithHarness with unknown tool name returns name unchanged (Custom fallback)
[<Fact>]
let ``A: renderWithHarness with unknown tool name returns name unchanged`` () =
    let result =
        Template.renderWithHarness
            "Use {{{tool UnknownTool}}}"
            testToolNameMap
            testToolToString
            Opencode
            Map.empty
    Assert.Equal("Use UnknownTool", result)

// 4.5 – renderWithHarness preserves non-tool template variables alongside tool substitutions
[<Fact>]
let ``A: renderWithHarness preserves non-tool variables alongside tool substitutions`` () =
    let vars = Map.ofList [("verb", "invoke" :> obj)]
    let result =
        Template.renderWithHarness
            "{{{verb}}} {{{tool Bash}}}"
            testToolNameMap
            testToolToString
            Opencode
            vars
    Assert.Equal("invoke bash", result)

// 4.6 – renderFileWithHarness resolves {{{tool Read}}} from a file for Copilot
[<Fact>]
let ``A: renderFileWithHarness resolves tool Read from file for Copilot harness`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "Use {{{tool Read}}}")
    let result =
        Template.renderFileWithHarness
            tempFile
            testToolNameMap
            testToolToString
            Copilot
            Map.empty
    File.Delete(tempFile)
    Assert.Equal("Use read", result)

// 4.7 – renderFileWithHarness returns "file not found" message when path does not exist
[<Fact>]
let ``A: renderFileWithHarness returns file not found message for missing file`` () =
    let result =
        Template.renderFileWithHarness
            "/nonexistent/template.txt"
            testToolNameMap
            testToolToString
            Opencode
            Map.empty
    Assert.Contains("[Template file not found:", result)
    Assert.Contains("/nonexistent/template.txt", result)
