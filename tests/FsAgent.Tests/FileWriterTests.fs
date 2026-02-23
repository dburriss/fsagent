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
open Testably.Abstractions.Testing

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
let ``A 7.2: resolveOutputPath raises NotSupportedException for Copilot + Global when env var not set`` () =
    let prev = Environment.GetEnvironmentVariable("COPILOT_GLOBAL_ROOT")
    try
        Environment.SetEnvironmentVariable("COPILOT_GLOBAL_ROOT", null)
        let ex = Assert.Throws<NotSupportedException>(fun () ->
            resolveOutputPath Copilot AgentArtifact "my-bot" Global |> ignore)
        Assert.Contains("Copilot", ex.Message)
    finally
        Environment.SetEnvironmentVariable("COPILOT_GLOBAL_ROOT", prev)

[<Fact>]
let ``A 7.3: resolveOutputPath SkillArtifact Project scope uses name/SKILL.md layout`` () =
    let root = "/repo"
    let scope = Project root

    let ocPath = resolveOutputPath Opencode SkillArtifact "my-skill" scope
    let cpPath = resolveOutputPath Copilot SkillArtifact "my-skill" scope
    let ccPath = resolveOutputPath ClaudeCode SkillArtifact "my-skill" scope

    // OpenCode project-scope skill now defaults to .agents/skills/ (cross-tool spec path)
    Assert.EndsWith(Path.Combine(".agents", "skills", "my-skill", "SKILL.md"), ocPath)
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

// ── 7.x Tests — FolderVariant option ─────────────────────────────────────────

[<Fact>]
let ``A 7.9: resolveOutputPath arity-4 default gives .agents/skills/ for OpenCode`` () =
    let path = resolveOutputPath Opencode SkillArtifact "my-skill" (Project "/repo")
    Assert.EndsWith(Path.Combine(".agents", "skills", "my-skill", "SKILL.md"), path)

[<Fact>]
let ``A 7.10: resolveOutputPathWith OpencodeFolder gives .opencode/skills/ for OpenCode`` () =
    let path = resolveOutputPathWith Opencode SkillArtifact "my-skill" (Project "/repo") OpencodeFolder
    Assert.EndsWith(Path.Combine(".opencode", "skills", "my-skill", "SKILL.md"), path)

[<Fact>]
let ``A 7.11: FolderVariant AgentsFolder and OpencodeFolder have no effect on ClaudeCode path`` () =
    let ccPathAgents   = resolveOutputPathWith ClaudeCode SkillArtifact "s" (Project "/r") AgentsFolder
    let ccPathOpencode = resolveOutputPathWith ClaudeCode SkillArtifact "s" (Project "/r") OpencodeFolder

    Assert.EndsWith(Path.Combine(".claude", "skills", "s", "SKILL.md"), ccPathAgents)
    Assert.EndsWith(Path.Combine(".claude", "skills", "s", "SKILL.md"), ccPathOpencode)

[<Fact>]
let ``A 7.19: resolveOutputPathWith ClaudeFolder gives .claude/skills/ for OpenCode`` () =
    let path = resolveOutputPathWith Opencode SkillArtifact "my-skill" (Project "/repo") ClaudeFolder
    Assert.EndsWith(Path.Combine(".claude", "skills", "my-skill", "SKILL.md"), path)

[<Fact>]
let ``A 7.20: resolveOutputPathWith ClaudeFolder gives .claude/skills/ for Copilot`` () =
    let path = resolveOutputPathWith Copilot SkillArtifact "my-skill" (Project "/repo") ClaudeFolder
    Assert.EndsWith(Path.Combine(".claude", "skills", "my-skill", "SKILL.md"), path)

[<Fact>]
let ``A 7.21: resolveOutputPathWith AgentsFolder and OpencodeFolder have no effect on Copilot path`` () =
    let cpPathAgents   = resolveOutputPathWith Copilot SkillArtifact "s" (Project "/r") AgentsFolder
    let cpPathOpencode = resolveOutputPathWith Copilot SkillArtifact "s" (Project "/r") OpencodeFolder

    Assert.EndsWith(Path.Combine(".github", "skills", "s", "SKILL.md"), cpPathAgents)
    Assert.EndsWith(Path.Combine(".github", "skills", "s", "SKILL.md"), cpPathOpencode)

// ── 7.x Tests — ConfigPaths module ───────────────────────────────────────────

[<Fact>]
let ``A 7.12: ConfigPaths.resolveProjectRoot returns harness subdirectory for all harnesses`` () =
    let ocPath = ConfigPaths.resolveProjectRoot Opencode "/repo"
    let cpPath = ConfigPaths.resolveProjectRoot Copilot "/repo"
    let ccPath = ConfigPaths.resolveProjectRoot ClaudeCode "/repo"

    Assert.EndsWith(Path.Combine(".opencode"), ocPath)
    Assert.EndsWith(Path.Combine(".github"), cpPath)
    Assert.EndsWith(Path.Combine(".claude"), ccPath)

[<Fact>]
let ``A 7.13: ConfigPaths.resolveGlobalRoot returns OS-correct path for Opencode`` () =
    let path = ConfigPaths.resolveGlobalRoot Opencode None
    Assert.EndsWith(Path.Combine(".config", "opencode"), path)

[<Fact>]
let ``A 7.14: ConfigPaths.resolveGlobalRoot returns .claude path for ClaudeCode`` () =
    let path = ConfigPaths.resolveGlobalRoot ClaudeCode None
    Assert.EndsWith(".claude", path)

[<Fact>]
let ``A 7.15: ConfigPaths.resolveGlobalRoot returns explicit root for Copilot`` () =
    let path = ConfigPaths.resolveGlobalRoot Copilot (Some "/path/to/private")
    Assert.Equal("/path/to/private", path)

[<Fact>]
let ``A 7.16: ConfigPaths.resolveGlobalRoot raises for Copilot when no env var set`` () =
    let prev = Environment.GetEnvironmentVariable("COPILOT_GLOBAL_ROOT")
    try
        Environment.SetEnvironmentVariable("COPILOT_GLOBAL_ROOT", null)
        Assert.Throws<NotSupportedException>(fun () ->
            ConfigPaths.resolveGlobalRoot Copilot None |> ignore)
        |> ignore
    finally
        Environment.SetEnvironmentVariable("COPILOT_GLOBAL_ROOT", prev)

// ── 7.x Tests — Copilot env var fallback ─────────────────────────────────────

[<Fact>]
let ``A 7.17: ConfigPaths.resolveGlobalRoot uses COPILOT_GLOBAL_ROOT env var when set`` () =
    let prev = Environment.GetEnvironmentVariable("COPILOT_GLOBAL_ROOT")
    try
        Environment.SetEnvironmentVariable("COPILOT_GLOBAL_ROOT", "/path/to/github-private")
        let path = ConfigPaths.resolveGlobalRoot Copilot None
        Assert.Equal("/path/to/github-private", path)
    finally
        Environment.SetEnvironmentVariable("COPILOT_GLOBAL_ROOT", prev)

[<Fact>]
let ``A 7.18: explicit copilotRoot overrides COPILOT_GLOBAL_ROOT env var`` () =
    let prev = Environment.GetEnvironmentVariable("COPILOT_GLOBAL_ROOT")
    try
        Environment.SetEnvironmentVariable("COPILOT_GLOBAL_ROOT", "/from-env")
        let path = ConfigPaths.resolveGlobalRoot Copilot (Some "/explicit")
        Assert.Equal("/explicit", path)
    finally
        Environment.SetEnvironmentVariable("COPILOT_GLOBAL_ROOT", prev)

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

// ── 9. Tests — AgentFileWriter (MockFileSystem, no real I/O) ─────────────────

[<Fact>]
let ``C 9.1: AgentFileWriter WriteAgent creates file at correct path in mock filesystem`` () =
    let fs = MockFileSystem()
    let writer = AgentFileWriter(fs, Project "/repo")
    let agent = mkAgent (Some "my-bot")

    let returned = writer.WriteAgent(agent, Opencode)

    let expected = resolveOutputPath Opencode AgentArtifact "my-bot" (Project "/repo")
    Assert.Equal(expected, returned)
    Assert.True(fs.File.Exists(returned), $"File not found in mock fs at: {returned}")

[<Fact>]
let ``C 9.2: AgentFileWriter WriteSkill for OpenCode defaults to .agents/skills/`` () =
    let fs = MockFileSystem()
    let writer = AgentFileWriter(fs, Project "/repo")
    let skill = mkSkill (Some "my-skill")

    let returned = writer.WriteSkill(skill, Opencode)

    Assert.EndsWith(Path.Combine(".agents", "skills", "my-skill", "SKILL.md"), returned)
    Assert.True(fs.File.Exists(returned), $"File not found in mock fs at: {returned}")

[<Fact>]
let ``C 9.3: AgentFileWriter WriteSkill for ClaudeCode uses .claude/skills/`` () =
    let fs = MockFileSystem()
    let writer = AgentFileWriter(fs, Project "/repo")
    let skill = mkSkill (Some "my-skill")

    let returned = writer.WriteSkill(skill, ClaudeCode)

    Assert.EndsWith(Path.Combine(".claude", "skills", "my-skill", "SKILL.md"), returned)
    Assert.True(fs.File.Exists(returned), $"File not found in mock fs at: {returned}")

[<Fact>]
let ``C 9.4: AgentFileWriter WriteCommand for Copilot uses .prompt.md`` () =
    let fs = MockFileSystem()
    let writer = AgentFileWriter(fs, Project "/repo")
    let cmd = mkCommand "deploy"

    let returned = writer.WriteCommand(cmd, Copilot)

    Assert.True(returned.EndsWith(".prompt.md"), $"Expected .prompt.md suffix, got: {returned}")
    Assert.True(fs.File.Exists(returned), $"File not found in mock fs at: {returned}")

[<Fact>]
let ``C 9.5: AgentFileWriter WriteCommand for ClaudeCode with namespace uses subdirectory`` () =
    let fs = MockFileSystem()
    let writer = AgentFileWriter(fs, Project "/repo")
    let cmd = mkCommand "apply"

    let returned = writer.WriteCommand(cmd, ClaudeCode, ns = "opsx")

    Assert.EndsWith(Path.Combine("opsx", "apply.md"), returned)
    Assert.True(fs.File.Exists(returned), $"File not found in mock fs at: {returned}")

[<Fact>]
let ``C 9.6: AgentFileWriter constructed with copilotRoot uses it for Copilot global scope`` () =
    let fs = MockFileSystem()
    let writer = AgentFileWriter(fs, Global, copilotRoot = "/ctor-root")
    let agent = mkAgent (Some "my-bot")

    let returned = writer.WriteAgent(agent, Copilot)

    Assert.True(returned.StartsWith("/ctor-root"), $"Expected path under /ctor-root, got: {returned}")
    Assert.True(fs.File.Exists(returned), $"File not found in mock fs at: {returned}")

[<Fact>]
let ``C 9.7: AgentFileWriter with ClaudeFolder writes Opencode skill to .claude/skills/`` () =
    let fs = MockFileSystem()
    let writer = AgentFileWriter(fs, Project "/repo", folderVariant = ClaudeFolder)
    let skill = mkSkill (Some "my-skill")

    let returned = writer.WriteSkill(skill, Opencode)

    Assert.EndsWith(Path.Combine(".claude", "skills", "my-skill", "SKILL.md"), returned)
    Assert.True(fs.File.Exists(returned), $"File not found in mock fs at: {returned}")

[<Fact>]
let ``C 9.8: AgentFileWriter with ClaudeFolder writes Copilot skill to .claude/skills/`` () =
    let fs = MockFileSystem()
    let writer = AgentFileWriter(fs, Project "/repo", folderVariant = ClaudeFolder)
    let skill = mkSkill (Some "my-skill")

    let returned = writer.WriteSkill(skill, Copilot)

    Assert.EndsWith(Path.Combine(".claude", "skills", "my-skill", "SKILL.md"), returned)
    Assert.True(fs.File.Exists(returned), $"File not found in mock fs at: {returned}")
