module DslTests

open Xunit
open FsAgent
open FsAgent.DSL

[<Fact>]
let ``DSL agent CE builds Agent with sections`` () =
    let agent = DSL.agent {
        role "You are a senior code review agent."
        objective "Focus on bugs, edge cases, and architectural risks."
    }
    Assert.Equal(2, agent.Sections.Length)
    match agent.Sections[0] with
    | Section("role", [Text t]) -> Assert.Equal("You are a senior code review agent.", t)
    | _ -> Assert.Fail("Expected role section")

[<Fact>]
let ``DSL meta builder supports kvObj and kvListObj`` () =
    let fm = DSL.meta {
        kvObj "model" (Map.ofList [("name", AST.fmStr "gpt-4"); ("temperature", AST.fmNum 0.2)])
        kvListObj "mcp-servers" [Map.ofList [("name", AST.fmStr "server1")] :> obj]
    }
    let agent = DSL.agent {
        meta fm
    }
    let model = agent.Frontmatter["model"] :?> Map<string, obj>
    Assert.Equal("gpt-4", model["name"] :?> string)
    Assert.Equal(0.2, model["temperature"] :?> float)

[<Fact>]
let ``DSL examples operation builds correct structure`` () =
    let agent = DSL.agent {
        examples [
            AST.example "Title1" "Content1"
            AST.example "Title2" "Content2"
        ]
    }
    match agent.Sections[0] with
    | Section("examples", [List [Section("example", _); Section("example", _)]]) -> ()
    | _ -> Assert.Fail("Expected examples section with list of examples")

[<Fact>]
let ``DSL import operation adds Imported node with wrapInCodeBlock true`` () =
    let agent = DSL.agent {
        import "data.yml"
    }
    match agent.Sections[0] with
    | Imported("data.yml", Yaml, true) -> ()
    | _ -> Assert.Fail("Expected Imported node with yaml format and wrapInCodeBlock=true")

[<Fact>]
let ``DSL importRaw operation adds Imported node with wrapInCodeBlock false`` () =
    let agent = DSL.agent {
        importRaw "data.json"
    }
    match agent.Sections[0] with
    | Imported("data.json", Json, false) -> ()
    | _ -> Assert.Fail("Expected Imported node with json format and wrapInCodeBlock=false")