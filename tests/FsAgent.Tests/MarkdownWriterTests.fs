module MarkdownWriterTests

open Xunit
open FsAgent
open FsAgent.DSL
open System.IO

// A - Acceptance Tests: End-to-end DSL → AST → Writer pipeline

[<Fact>]
let ``A: Default writeMarkdown produces Markdown with ATX headings and frontmatter`` () =
    let agent = {
        Frontmatter = Map.ofList ["description", "Test agent" :> obj]
        Sections = [
            AST.role "You are a test agent"
            AST.objective "Test objective"
        ]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    Assert.Contains("# role", result)
    Assert.Contains("# objective", result)
    Assert.Contains("---", result)
    Assert.Contains("description: Test agent", result)

[<Fact>]
let ``A: Copilot format includes name and description in frontmatter`` () =
    let agent = {
        Frontmatter = Map.ofList ["name", "TestAgent" :> obj; "description", "Test desc" :> obj]
        Sections = [AST.role "Role"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts -> opts.OutputFormat <- MarkdownWriter.Copilot)
    Assert.Contains("name: TestAgent", result)
    Assert.Contains("description: Test desc", result)

[<Fact>]
let ``A: importRaw embeds file content without code fences`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "Imported content")
    let agent = {
        Frontmatter = Map.empty
        Sections = [Imported(tempFile, Yaml, false)]  // wrapInCodeBlock = false
    }
    let result = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    Assert.Contains("Imported content", result)
    Assert.DoesNotContain("```", result)
    File.Delete(tempFile)

[<Fact>]
let ``A: Heading rename map applies correctly`` () =
    let agent = {
        Frontmatter = Map.empty
        Sections = [AST.role "Role text"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts -> opts.RenameMap <- Map.ofList ["role", "Agent Role"])
    Assert.Contains("# Agent Role", result)

[<Fact>]
let ``A: Heading formatter applies after renames`` () =
    let agent = {
        Frontmatter = Map.empty
        Sections = [AST.role "Role text"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts ->
        opts.RenameMap <- Map.ofList ["role", "agent role"]
        opts.HeadingFormatter <- Some (fun s -> s.ToUpper()))
    Assert.Contains("# AGENT ROLE", result)

[<Fact>]
let ``A: Footer generator appends content`` () =
    let agent = {
        Frontmatter = Map.empty
        Sections = [AST.role "Role"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts ->
        opts.GeneratedFooter <- Some (fun ctx -> "Footer text"))
    Assert.Contains("Footer text", result)

[<Fact>]
let ``A: Output type JSON produces JSON`` () =
    let agent = {
        Frontmatter = Map.ofList ["key", "value" :> obj]
        Sections = [AST.role "Role"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts -> opts.OutputType <- MarkdownWriter.Json)
    Assert.Contains("\"frontmatter\"", result)
    Assert.Contains("\"sections\"", result)

[<Fact>]
let ``A: Output type YAML produces YAML`` () =
    let agent = {
        Frontmatter = Map.ofList ["key", "value" :> obj]
        Sections = [AST.role "Role"]
    }
    let result = MarkdownWriter.writeMarkdown agent (fun opts -> opts.OutputType <- MarkdownWriter.Yaml)
    Assert.Contains("frontmatter:", result)
    Assert.Contains("sections:", result)

[<Fact>]
let ``A: Custom writer overrides default`` () =
    let agent = { Frontmatter = Map.empty; Sections = [] }
    let result = MarkdownWriter.writeMarkdown agent (fun opts ->
        opts.CustomWriter <- Some (fun _ _ -> "Custom output"))
    Assert.Equal("Custom output", result)

[<Fact>]
let ``A: Deterministic output for same agent and options`` () =
    let agent = { Frontmatter = Map.empty; Sections = [AST.role "Role"] }
    let result1 = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    let result2 = MarkdownWriter.writeMarkdown agent (fun _ -> ())
    Assert.Equal(result1, result2)

[<Fact>]
let ``A: import wraps JSON in json fence by default`` () =
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, """{"key": "value"}""")
    let agent = {
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
    let agent = {
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
    let agent = {
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
    let agent = {
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
    let agent = {
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
    let agent = { Frontmatter = Map.empty; Sections = [] }
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