module FsAgent.Tests.SkillTests

open Xunit
open FsAgent
open FsAgent.Skills
open FsAgent.Prompts
open FsAgent.Writers
open FsAgent.AST

// A – Acceptance Tests: Skill DSL and renderSkill writer

// ── 5.1 Skill.empty ──────────────────────────────────────────────────────────

[<Fact>]
let ``A: Skill.empty has empty frontmatter and sections`` () =
    let s = Skill.empty
    Assert.Empty(s.Frontmatter)
    Assert.Empty(s.Sections)

// ── 5.2 skill { () } equals Skill.empty ──────────────────────────────────────

[<Fact>]
let ``A: skill { () } equals Skill.empty`` () =
    let s = skill { () }
    Assert.Equal(Skill.empty, s)

// ── 5.3 Frontmatter CE operations ────────────────────────────────────────────

[<Fact>]
let ``A: skill name sets frontmatter name key`` () =
    let s = skill { name "my-skill" }
    Assert.True(s.Frontmatter.ContainsKey("name"))
    Assert.Equal("my-skill" :> obj, s.Frontmatter.["name"])

[<Fact>]
let ``A: skill description sets frontmatter description key`` () =
    let s = skill { description "Does X" }
    Assert.Equal("Does X" :> obj, s.Frontmatter.["description"])

[<Fact>]
let ``A: skill license sets frontmatter license key`` () =
    let s = skill { license "MIT" }
    Assert.Equal("MIT" :> obj, s.Frontmatter.["license"])

[<Fact>]
let ``A: skill compatibility sets frontmatter compatibility key`` () =
    let s = skill { compatibility "Requires openspec CLI." }
    Assert.Equal("Requires openspec CLI." :> obj, s.Frontmatter.["compatibility"])

[<Fact>]
let ``A: skill metadata sets frontmatter metadata key as Map`` () =
    let meta = Map.ofList ["author", box "alice"; "version", box "1.0"]
    let s = skill { metadata meta }
    Assert.True(s.Frontmatter.ContainsKey("metadata"))
    let stored = s.Frontmatter.["metadata"] :?> Map<string, obj>
    Assert.Equal(box "alice", stored.["author"])
    Assert.Equal(box "1.0", stored.["version"])

// ── 5.4 section operation ────────────────────────────────────────────────────

[<Fact>]
let ``A: section appends correct Section node`` () =
    let s = skill { section "Overview" "This skill does X." }
    Assert.Equal(1, s.Sections.Length)
    match s.Sections.[0] with
    | Section("Overview", [Text "This skill does X."]) -> ()
    | other -> Assert.Fail($"Unexpected node: {other}")

// ── 5.5 prompt operation ─────────────────────────────────────────────────────

[<Fact>]
let ``A: prompt appends prompt sections to skill sections`` () =
    let p = prompt { role "You are an assistant" }
    let s = skill { prompt p }
    Assert.Equal(p.Sections.Length, s.Sections.Length)
    Assert.Equal(p.Sections.[0], s.Sections.[0])

// ── 5.6 template and templateFile operations ──────────────────────────────────

[<Fact>]
let ``A: template appends Template node`` () =
    let s = skill { template "Hello {{{name}}}" }
    Assert.Equal(1, s.Sections.Length)
    match s.Sections.[0] with
    | Template "Hello {{{name}}}" -> ()
    | other -> Assert.Fail($"Unexpected node: {other}")

[<Fact>]
let ``A: templateFile appends TemplateFile node`` () =
    let s = skill { templateFile "/path/to/file.md" }
    Assert.Equal(1, s.Sections.Length)
    match s.Sections.[0] with
    | TemplateFile "/path/to/file.md" -> ()
    | other -> Assert.Fail($"Unexpected node: {other}")

// ── 5.7 renderSkill frontmatter block ────────────────────────────────────────

[<Fact>]
let ``A: renderSkill produces frontmatter block with all keys`` () =
    let meta = Map.ofList ["author", box "alice"; "version", box "1.0"]
    let s =
        skill {
            name "my-skill"
            description "Does things"
            license "MIT"
            compatibility "openspec >= 1.0"
            metadata meta
        }
    let result = AgentWriter.renderSkill s AgentWriter.Opencode (fun _ -> ())
    Assert.Contains("---", result)
    Assert.Contains("name: my-skill", result)
    Assert.Contains("description: Does things", result)
    Assert.Contains("license: MIT", result)
    Assert.Contains("compatibility: openspec >= 1.0", result)
    Assert.Contains("metadata:", result)

// ── 5.8 metadata nested YAML mapping ─────────────────────────────────────────

[<Fact>]
let ``A: renderSkill serializes metadata map as nested YAML mapping`` () =
    let meta = Map.ofList ["author", box "alice"; "version", box "1.0"]
    let s = skill { name "meta-skill"; description "Metadata test"; metadata meta }
    let result = AgentWriter.renderSkill s AgentWriter.Opencode (fun _ -> ())
    Assert.Contains("metadata:", result)
    Assert.Contains("  author: alice", result)
    Assert.Contains("  version: 1.0", result)

// ── 5.9 section rendered as heading ──────────────────────────────────────────

[<Fact>]
let ``A: renderSkill renders Section node as # Heading`` () =
    let s = skill { name "section-skill"; description "Section test"; section "Steps" "Do this." }
    let result = AgentWriter.renderSkill s AgentWriter.Opencode (fun _ -> ())
    Assert.Contains("# Steps", result)
    Assert.Contains("Do this.", result)

// ── 5.10 tool name resolution per harness ─────────────────────────────────────

[<Fact>]
let ``A: renderSkill resolves tool Bash as 'bash' for Opencode harness`` () =
    let s = skill { name "bash-skill"; description "Bash test"; template "{{{tool Bash}}}" }
    let result = AgentWriter.renderSkill s AgentWriter.Opencode (fun _ -> ())
    Assert.Contains("bash", result)
    Assert.DoesNotContain("{{{tool Bash}}}", result)

[<Fact>]
let ``A: renderSkill resolves tool Bash as 'Bash' for ClaudeCode harness`` () =
    let s = skill { name "bash-skill"; description "Bash test"; template "{{{tool Bash}}}" }
    let result = AgentWriter.renderSkill s AgentWriter.ClaudeCode (fun _ -> ())
    Assert.Contains("Bash", result)
    Assert.DoesNotContain("{{{tool Bash}}}", result)

// ── 5.11 empty skill raises validation error ──────────────────────────────────

[<Fact>]
let ``C: renderSkill raises when name is missing`` () =
    let s : Skill = { Frontmatter = Map.ofList ["description", box "desc"]; Sections = [] }
    let ex = Assert.Throws<AgentWriter.ValidationException>(fun () ->
        AgentWriter.renderSkill s AgentWriter.Opencode (fun _ -> ()) |> ignore)
    Assert.Contains("'name'", ex.Message)

[<Fact>]
let ``C: renderSkill reports all errors when name and description are both missing`` () =
    let ex = Assert.Throws<AgentWriter.ValidationException>(fun () ->
        AgentWriter.renderSkill Skill.empty AgentWriter.Opencode (fun _ -> ()) |> ignore)
    Assert.Contains("'name'", ex.Message)
    Assert.Contains("'description'", ex.Message)

// ── 5.12 HeadingFormatter option ─────────────────────────────────────────────

[<Fact>]
let ``A: renderSkill respects HeadingFormatter option`` () =
    let s = skill { name "heading-skill"; description "Heading test"; section "overview" "Details here." }
    let result = AgentWriter.renderSkill s AgentWriter.Opencode (fun opts ->
        opts.HeadingFormatter <- Some (fun s -> s.ToUpper()))
    Assert.Contains("# OVERVIEW", result)

// ── 5.13 TemplateVariables option ────────────────────────────────────────────

[<Fact>]
let ``A: renderSkill respects TemplateVariables option`` () =
    let s = skill { name "tpl-skill"; description "Template test"; template "Hello {{{name}}}" }
    let result = AgentWriter.renderSkill s AgentWriter.Opencode (fun opts ->
        opts.TemplateVariables <- Map.ofList ["name", box "world"])
    Assert.Contains("Hello world", result)

// ── 5.14 renderAgent output unchanged ────────────────────────────────────────

[<Fact>]
let ``A: renderAgent output is unchanged after adding renderSkill`` () =
    let agent: Agent = {
        Frontmatter = Map.ofList ["description", box "Test agent"]
        Sections = [
            Prompt.role "You are a test agent"
            Prompt.objective "Test objective"
        ]
    }
    let result = AgentWriter.renderAgent agent (fun _ -> ())
    Assert.Contains("# role", result)
    Assert.Contains("# objective", result)
    Assert.Contains("---", result)
    Assert.Contains("description: Test agent", result)
