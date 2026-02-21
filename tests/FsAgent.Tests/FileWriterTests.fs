module FileWriterTests

open System
open System.IO
open Xunit
open FsAgent
open FsAgent.Agents
open FsAgent.Skills
open FsAgent.Commands
open FsAgent.Writers
open FsAgent.Writers.FileWriter

// ── Helpers ──────────────────────────────────────────────────────────────────

let private mkAgent name =
    let agent: Agent = {
        Frontmatter =
            match name with
            | Some n -> Map.ofList [ "name", n :> obj; "description", "test agent" :> obj ]
            | None   -> Map.ofList [ "description", "test agent" :> obj ]
        Sections = []
    }
    agent

let private mkSkill name =
    let skill: Skill = {
        Frontmatter =
            match name with
            | Some n -> Map.ofList [ "name", n :> obj; "description", "test skill" :> obj ]
            | None   -> Map.ofList [ "description", "test skill" :> obj ]
        Sections = []
    }
    skill

let private mkCommand name =
    let cmd: SlashCommand = {
        Name = name
        Description = "test command"
        Sections = []
    }
    cmd

// ── 7. Tests — Path Resolution (pure, no I/O) ────────────────────────────────

[<Fact>]
let ``A 7.1: resolveOutputPath AgentArtifact Project scope for all three harnesses`` () =
    let root = "/repo"
    let scope = Project root

    let ocPath = resolveOutputPath Opencode AgentArtifact "my-bot" scope
    let cpPath = resolveOutputPath Copilot AgentArtifact "my-bot" scope
    let ccPath = resolveOutputPath ClaudeCode AgentArtifact "my-bot" scope

    Assert.EndsWith(Path.Combine(".opencode", "agents", "my-bot.md"), ocPath)
    Assert.EndsWith(Path.Combine(".github", "agents", "my-bot.md"), cpPath)
    Assert.EndsWith(Path.Combine(".claude", "agents", "my-bot.md"), ccPath)

[<Fact>]
let ``A 7.2: resolveOutputPath raises NotSupportedException for Copilot + Global`` () =
    let ex = Assert.Throws<NotSupportedException>(fun () ->
        resolveOutputPath Copilot AgentArtifact "my-bot" Global |> ignore)
    Assert.Contains("Copilot", ex.Message)

[<Fact>]
let ``A 7.3: resolveOutputPath SkillArtifact Project scope uses name/SKILL.md layout`` () =
    let root = "/repo"
    let scope = Project root

    let ocPath = resolveOutputPath Opencode SkillArtifact "my-skill" scope
    let cpPath = resolveOutputPath Copilot SkillArtifact "my-skill" scope
    let ccPath = resolveOutputPath ClaudeCode SkillArtifact "my-skill" scope

    Assert.EndsWith(Path.Combine(".opencode", "skills", "my-skill", "SKILL.md"), ocPath)
    Assert.EndsWith(Path.Combine(".github", "skills", "my-skill", "SKILL.md"), cpPath)
    Assert.EndsWith(Path.Combine(".claude", "skills", "my-skill", "SKILL.md"), ccPath)

[<Fact>]
let ``A 7.4: resolveOutputPath Global scope Opencode and ClaudeCode skill paths end with expected suffix`` () =
    let ocPath = resolveOutputPath Opencode SkillArtifact "my-skill" Global
    let ccPath = resolveOutputPath ClaudeCode SkillArtifact "my-skill" Global

    Assert.EndsWith(Path.Combine("opencode", "skills", "my-skill", "SKILL.md"), ocPath)
    Assert.EndsWith(Path.Combine(".claude", "skills", "my-skill", "SKILL.md"), ccPath)

[<Fact>]
let ``A 7.5: resolveOutputPath CommandArtifact None Project scope for Opencode and ClaudeCode`` () =
    let root = "/repo"
    let scope = Project root

    let ocPath = resolveOutputPath Opencode (CommandArtifact None) "deploy" scope
    let ccPath = resolveOutputPath ClaudeCode (CommandArtifact None) "deploy" scope

    Assert.EndsWith(Path.Combine(".opencode", "commands", "deploy.md"), ocPath)
    Assert.EndsWith(Path.Combine(".claude", "commands", "deploy.md"), ccPath)

[<Fact>]
let ``A 7.6: resolveOutputPath Copilot command returns path ending in .prompt.md`` () =
    let path = resolveOutputPath Copilot (CommandArtifact None) "deploy" (Project "/repo")

    Assert.True(path.EndsWith(".prompt.md"), $"Expected .prompt.md suffix, got: {path}")
    Assert.EndsWith(Path.Combine(".github", "prompts", "deploy.prompt.md"), path)

[<Fact>]
let ``A 7.7: resolveOutputPath ClaudeCode namespaced command uses subdirectory`` () =
    let path =
        resolveOutputPath ClaudeCode (CommandArtifact (Some "opsx")) "apply" (Project "/repo")

    Assert.EndsWith(Path.Combine(".claude", "commands", "opsx", "apply.md"), path)

[<Fact>]
let ``A 7.8: resolveOutputPath with Project "." produces an absolute path`` () =
    let path = resolveOutputPath Opencode AgentArtifact "bot" (Project ".")

    Assert.False(path.StartsWith("."), $"Expected absolute path, got: {path}")
    Assert.True(Path.IsPathRooted(path), $"Expected rooted path, got: {path}")

// ── 8. Tests — File I/O (filesystem, temp directory) ─────────────────────────

[<Fact>]
let ``C 8.1: writeFile creates file at resolved path in temp directory`` () =
    let tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    try
        let content = "hello agent"
        writeFile Opencode AgentArtifact "bot" (Project tmpRoot) content |> ignore

        let expected = resolveOutputPath Opencode AgentArtifact "bot" (Project tmpRoot)
        Assert.True(File.Exists(expected), $"File not found at: {expected}")
        Assert.Equal(content, File.ReadAllText(expected))
    finally
        if Directory.Exists(tmpRoot) then Directory.Delete(tmpRoot, true)

[<Fact>]
let ``C 8.2: writeFile creates intermediate directories that do not exist`` () =
    let tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    Assert.False(Directory.Exists(tmpRoot))
    try
        writeFile Opencode AgentArtifact "bot" (Project tmpRoot) "content" |> ignore
        let expected = resolveOutputPath Opencode AgentArtifact "bot" (Project tmpRoot)
        let dir = Path.GetDirectoryName(expected)
        Assert.True(Directory.Exists(dir), $"Directory not created: {dir}")
    finally
        if Directory.Exists(tmpRoot) then Directory.Delete(tmpRoot, true)

[<Fact>]
let ``C 8.3: writeFile overwrites an existing file with new content`` () =
    let tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    try
        let scope = Project tmpRoot
        writeFile Opencode AgentArtifact "bot" scope "original" |> ignore
        writeFile Opencode AgentArtifact "bot" scope "updated" |> ignore

        let expected = resolveOutputPath Opencode AgentArtifact "bot" scope
        Assert.Equal("updated", File.ReadAllText(expected))
    finally
        if Directory.Exists(tmpRoot) then Directory.Delete(tmpRoot, true)

[<Fact>]
let ``C 8.4: writeFile returns the resolved path string`` () =
    let tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    try
        let scope = Project tmpRoot
        let returned = writeFile Opencode AgentArtifact "bot" scope "content"
        let expected = resolveOutputPath Opencode AgentArtifact "bot" scope
        Assert.Equal(expected, returned)
    finally
        if Directory.Exists(tmpRoot) then Directory.Delete(tmpRoot, true)

[<Fact>]
let ``C 8.5: writeAgent raises ArgumentException when Agent has no name in frontmatter`` () =
    let tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    try
        let agent = mkAgent None
        Assert.Throws<ArgumentException>(fun () ->
            writeAgent agent Opencode (Project tmpRoot) (fun _ -> ()) |> ignore)
        |> ignore
    finally
        if Directory.Exists(tmpRoot) then Directory.Delete(tmpRoot, true)

[<Fact>]
let ``C 8.6: writeAgent writes to correct path and content matches rendered output`` () =
    let tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    try
        let agent = mkAgent (Some "my-bot")
        let scope = Project tmpRoot
        let returned = writeAgent agent Opencode scope (fun _ -> ())

        let expected = resolveOutputPath Opencode AgentArtifact "my-bot" scope
        Assert.Equal(expected, returned)
        Assert.True(File.Exists(expected), $"File not found at: {expected}")

        let rendered = AgentWriter.renderAgent agent (fun opts -> opts.OutputFormat <- Opencode)
        Assert.Equal(rendered, File.ReadAllText(expected))
    finally
        if Directory.Exists(tmpRoot) then Directory.Delete(tmpRoot, true)

[<Fact>]
let ``C 8.7: writeSkill raises ArgumentException when Skill has no name in frontmatter`` () =
    let tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    try
        let skill = mkSkill None
        Assert.Throws<ArgumentException>(fun () ->
            writeSkill skill Opencode (Project tmpRoot) (fun _ -> ()) |> ignore)
        |> ignore
    finally
        if Directory.Exists(tmpRoot) then Directory.Delete(tmpRoot, true)

[<Fact>]
let ``C 8.8: writeSkill writes to correct name/SKILL.md subdirectory layout`` () =
    let tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    try
        let skill = mkSkill (Some "my-skill")
        let scope = Project tmpRoot
        let returned = writeSkill skill Opencode scope (fun _ -> ())

        let expected = resolveOutputPath Opencode SkillArtifact "my-skill" scope
        Assert.Equal(expected, returned)
        Assert.True(File.Exists(expected), $"File not found at: {expected}")
        Assert.True(expected.EndsWith(Path.Combine("my-skill", "SKILL.md")), $"Path should end with my-skill/SKILL.md: {expected}")
    finally
        if Directory.Exists(tmpRoot) then Directory.Delete(tmpRoot, true)

[<Fact>]
let ``C 8.9: writeCommand for Copilot writes file with .prompt.md extension`` () =
    let tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    try
        let cmd = mkCommand "deploy"
        let scope = Project tmpRoot
        let returned = writeCommand cmd Copilot scope None (fun _ -> ())

        Assert.True(returned.EndsWith(".prompt.md"), $"Expected .prompt.md suffix, got: {returned}")
        Assert.True(File.Exists(returned), $"File not found at: {returned}")
    finally
        if Directory.Exists(tmpRoot) then Directory.Delete(tmpRoot, true)

[<Fact>]
let ``C 8.10: writeCommand for ClaudeCode with namespace writes to correct subdirectory`` () =
    let tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    try
        let cmd = mkCommand "apply"
        let scope = Project tmpRoot
        let returned = writeCommand cmd ClaudeCode scope (Some "opsx") (fun _ -> ())

        let expected = resolveOutputPath ClaudeCode (CommandArtifact (Some "opsx")) "apply" scope
        Assert.Equal(expected, returned)
        Assert.True(returned.EndsWith(Path.Combine("opsx", "apply.md")), $"Expected opsx/apply.md suffix, got: {returned}")
        Assert.True(File.Exists(returned), $"File not found at: {returned}")
    finally
        if Directory.Exists(tmpRoot) then Directory.Delete(tmpRoot, true)
