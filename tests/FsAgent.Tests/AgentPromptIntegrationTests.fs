module AgentPromptIntegrationTests

open Xunit
open FsAgent
open FsAgent.Agents
open FsAgent.Prompts
open FsAgent.Writers
open FsAgent.AST

[<Fact>]
let ``Agent references prompt via prompt operation`` () =
    let p = prompt {
        role "You are a helpful assistant"
        objective "Answer questions accurately"
    }
    let a = agent {
        name "TestAgent"
        prompt p
    }
    Assert.Equal(2, a.Sections.Length)
    Assert.Equal("TestAgent", a.Frontmatter["name"] :?> string)

[<Fact>]
let ``Multiple prompts referenced in single agent`` () =
    let p1 = prompt {
        role "You are an expert"
    }
    let p2 = prompt {
        objective "Provide detailed answers"
    }
    let p3 = prompt {
        output "Clear, concise responses"
    }
    let a = agent {
        prompt p1
        prompt p2
        prompt p3
    }
    Assert.Equal(3, a.Sections.Length)
    match a.Sections[0] with
    | Section("role", _) -> ()
    | _ -> Assert.Fail("Expected role from first prompt")
    match a.Sections[1] with
    | Section("objective", _) -> ()
    | _ -> Assert.Fail("Expected objective from second prompt")
    match a.Sections[2] with
    | Section("output", _) -> ()
    | _ -> Assert.Fail("Expected output from third prompt")

[<Fact>]
let ``Prompt sections merged into agent sections`` () =
    let p = prompt {
        role "Assistant"
        instructions "Be helpful and friendly"
    }
    let a = agent {
        name "MyAgent"
        prompt p
        section "additional" "Extra content"
    }
    Assert.Equal(3, a.Sections.Length)
    match a.Sections[0] with
    | Section("role", _) -> ()
    | _ -> Assert.Fail("Expected role from prompt")
    match a.Sections[1] with
    | Section("instructions", _) -> ()
    | _ -> Assert.Fail("Expected instructions from prompt")
    match a.Sections[2] with
    | Section("additional", _) -> ()
    | _ -> Assert.Fail("Expected additional section")

[<Fact>]
let ``Agent with prompt writes complete markdown`` () =
    let p = prompt {
        role "Expert advisor"
        objective "Provide guidance"
    }
    let a = agent {
        name "AdvisorAgent"
        description "An expert advisory agent"
        prompt p
    }
    let result = MarkdownWriter.writeAgent a (fun _ -> ())
    Assert.Contains("name: AdvisorAgent", result)
    Assert.Contains("# role", result)
    Assert.Contains("# objective", result)

[<Fact>]
let ``Prompt with templates integrated into agent`` () =
    let p = prompt {
        role "You are an assistant"
        template "Instructions: {{{instructions}}}"
    }
    let a = agent {
        prompt p
    }
    let result = MarkdownWriter.writeAgent a (fun opts ->
        opts.TemplateVariables <- Map.ofList [
            ("instructions", "check for bugs" :> obj)
        ])
    Assert.Contains("You are an assistant", result)
    Assert.Contains("check for bugs", result)

[<Fact>]
let ``Agent can combine multiple prompts with different concerns`` () =
    let systemPrompt = prompt {
        role "You are an AI assistant"
    }
    let taskPrompt = prompt {
        objective "Help with programming tasks"
        instructions "Provide code examples when helpful"
    }
    let outputPrompt = prompt {
        output "Format responses in markdown"
    }
    let a = agent {
        name "ProgrammingAssistant"
        prompt systemPrompt
        prompt taskPrompt
        prompt outputPrompt
    }
    Assert.Equal(4, a.Sections.Length)
    let result = MarkdownWriter.writeAgent a (fun _ -> ())
    Assert.Contains("# role", result)
    Assert.Contains("# objective", result)
    Assert.Contains("# instructions", result)
    Assert.Contains("# output", result)
