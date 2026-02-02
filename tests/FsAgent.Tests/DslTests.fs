module DslTests

open Xunit
open FsAgent
open FsAgent.Agents
open FsAgent.Prompts
open FsAgent.AST
open FsAgent.Tools

[<Fact>]
let ``DSL meta builder supports kvObj and kvListObj`` () =
    let fm = meta {
        kvObj "model" (Map.ofList [("name", AST.fmStr "gpt-4"); ("temperature", AST.fmNum 0.2)])
        kvListObj "mcp-servers" [Map.ofList [("name", AST.fmStr "server1")] :> obj]
    }
    let agent = agent {
        meta fm
    }
    let model = agent.Frontmatter["model"] :?> Map<string, obj>
    Assert.Equal("gpt-4", model["name"] :?> string)
    Assert.Equal(0.2, model["temperature"] :?> float)

[<Fact>]
let ``Agent builder supports model operation`` () =
    let a = agent {
        model "gpt-4"
    }
    Assert.Equal("gpt-4", a.Frontmatter["model"] :?> string)

[<Fact>]
let ``Agent builder supports temperature operation`` () =
    let a = agent {
        temperature 0.7
    }
    Assert.Equal(0.7, a.Frontmatter["temperature"] :?> float)

[<Fact>]
let ``Agent builder supports maxTokens operation`` () =
    let a = agent {
        maxTokens 2000.0
    }
    Assert.Equal(2000.0, a.Frontmatter["maxTokens"] :?> float)

[<Fact>]
let ``Agent builder supports tools operation`` () =
    let a = agent {
        tools [Custom "tool1"; Custom "tool2"]
    }
    let toolsList = a.Frontmatter["tools"] :?> Tool list
    Assert.Equal(2, toolsList.Length)
    Assert.Contains(Custom "tool1", toolsList)
    Assert.Contains(Custom "tool2", toolsList)

[<Fact>]
let ``Agent builder supports prompt operation merging sections`` () =
    let p = prompt {
        role "You are a helpful assistant"
        objective "Answer questions accurately"
    }
    let a = agent {
        prompt p
    }
    Assert.Equal(2, a.Sections.Length)
    match a.Sections[0] with
    | Section("role", [Text t]) -> Assert.Equal("You are a helpful assistant", t)
    | _ -> Assert.Fail("Expected role section")
    match a.Sections[1] with
    | Section("objective", [Text t]) -> Assert.Equal("Answer questions accurately", t)
    | _ -> Assert.Fail("Expected objective section")

[<Fact>]
let ``Agent builder supports multiple prompts`` () =
    let p1 = prompt {
        role "You are a code reviewer"
    }
    let p2 = prompt {
        objective "Find bugs and issues"
    }
    let a = agent {
        prompt p1
        prompt p2
    }
    Assert.Equal(2, a.Sections.Length)

[<Fact>]
let ``DSL import operation adds Imported node with wrapInCodeBlock true`` () =
    let a = agent {
        import "data.yml"
    }
    match a.Sections[0] with
    | Imported("data.yml", Yaml, true) -> ()
    | _ -> Assert.Fail("Expected Imported node with yaml format and wrapInCodeBlock=true")

[<Fact>]
let ``DSL importRaw operation adds Imported node with wrapInCodeBlock false`` () =
    let a = agent {
        importRaw "data.json"
    }
    match a.Sections[0] with
    | Imported("data.json", Json, false) -> ()
    | _ -> Assert.Fail("Expected Imported node with json format and wrapInCodeBlock=false")
