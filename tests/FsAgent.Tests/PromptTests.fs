module PromptTests

open Xunit
open FsAgent
open FsAgent.Prompts
open FsAgent.Writers
open FsAgent.AST

// A - Acceptance Tests: prompt DSL → Prompt → writePrompt pipeline

[<Fact>]
let ``A: prompt builder creates Prompt with role and objective`` () =
    let p = prompt {
        role "You are a helpful assistant"
        objective "Answer user questions accurately"
    }
    Assert.Equal(2, p.Sections.Length)
    match p.Sections[0] with
    | Section("role", [Text t]) -> Assert.Equal("You are a helpful assistant", t)
    | _ -> Assert.Fail("Expected role section")

[<Fact>]
let ``A: writePrompt produces markdown without frontmatter block`` () =
    let p = prompt {
        name "TestPrompt"
        description "A test prompt"
        role "You are a test"
    }
    let result = MarkdownWriter.writePrompt p (fun _ -> ())
    Assert.DoesNotContain("---", result)
    Assert.DoesNotContain("name:", result)
    Assert.Contains("# role", result)

[<Fact>]
let ``A: prompt with all content operations`` () =
    let p = prompt {
        role "Assistant"
        objective "Help"
        instructions "Be helpful"
        context "In a chat"
        output "Friendly responses"
    }
    Assert.Equal(5, p.Sections.Length)

// B - Building Tests: PromptBuilder operations

[<Fact>]
let ``B: prompt builder empty creates empty Prompt`` () =
    let p = prompt { () }
    Assert.Equal(0, p.Sections.Length)
    Assert.True(p.Frontmatter.IsEmpty)

[<Fact>]
let ``B: prompt builder name operation adds to frontmatter`` () =
    let p = prompt {
        name "TestPrompt"
    }
    Assert.Equal("TestPrompt", p.Frontmatter["name"] :?> string)

[<Fact>]
let ``B: prompt builder description operation adds to frontmatter`` () =
    let p = prompt {
        description "A description"
    }
    Assert.Equal("A description", p.Frontmatter["description"] :?> string)

[<Fact>]
let ``B: prompt builder metadata operations`` () =
    let p = prompt {
        author "Alice"
        version "1.0.0"
        license "MIT"
    }
    Assert.Equal("Alice", p.Frontmatter["author"] :?> string)
    Assert.Equal("1.0.0", p.Frontmatter["version"] :?> string)
    Assert.Equal("MIT", p.Frontmatter["license"] :?> string)

[<Fact>]
let ``B: prompt builder section operation creates custom section`` () =
    let p = prompt {
        section "custom" "Custom content"
    }
    match p.Sections[0] with
    | Section("custom", [Text t]) -> Assert.Equal("Custom content", t)
    | _ -> Assert.Fail("Expected custom section")

[<Fact>]
let ``B: prompt builder import operations`` () =
    let p = prompt {
        import "data.yml"
        importRaw "data.json"
    }
    Assert.Equal(2, p.Sections.Length)
    match p.Sections[0] with
    | Imported(_, _, true) -> ()
    | _ -> Assert.Fail("Expected import with wrap=true")
    match p.Sections[1] with
    | Imported(_, _, false) -> ()
    | _ -> Assert.Fail("Expected import with wrap=false")

[<Fact>]
let ``B: prompt builder template operations`` () =
    let p = prompt {
        template "Hello {{{name}}}"
        templateFile "template.txt"
    }
    Assert.Equal(2, p.Sections.Length)
    match p.Sections[0] with
    | Template t -> Assert.Equal("Hello {{{name}}}", t)
    | _ -> Assert.Fail("Expected Template node")
    match p.Sections[1] with
    | TemplateFile path -> Assert.Equal("template.txt", path)
    | _ -> Assert.Fail("Expected TemplateFile node")

// C - Communication Tests: prompt format validation

[<Fact>]
let ``C: writePrompt with JSON output type`` () =
    let p = prompt {
        role "Test"
    }
    let result = MarkdownWriter.writePrompt p (fun opts -> opts.OutputType <- MarkdownWriter.Json)
    Assert.Contains("\"sections\"", result)

[<Fact>]
let ``C: writePrompt with YAML output type`` () =
    let p = prompt {
        role "Test"
    }
    let result = MarkdownWriter.writePrompt p (fun opts -> opts.OutputType <- MarkdownWriter.Yaml)
    Assert.Contains("sections:", result)
