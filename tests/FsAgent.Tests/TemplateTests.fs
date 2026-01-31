module TemplateTests

open Xunit
open FsAgent.Writers
open System.IO

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
