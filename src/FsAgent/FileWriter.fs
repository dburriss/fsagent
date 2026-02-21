namespace FsAgent.Writers

open System
open System.IO
open System.Runtime.InteropServices
open FsAgent.Agents
open FsAgent.Skills
open FsAgent.Commands

/// Specifies whether to write to a project-local directory or the user's global config.
type WriteScope =
    /// Write relative to the given root directory (e.g. the repo root).
    | Project of rootDir: string
    /// Write to the OS-specific user config directory for the harness.
    | Global

/// Identifies the type of artifact being written.
type ArtifactKind =
    | AgentArtifact
    /// Slash command. The optional string is a ClaudeCode namespace subdirectory
    /// (e.g. `Some "opsx"` → `.claude/commands/opsx/<name>.md`).
    | CommandArtifact of namespace_: string option
    | SkillArtifact

module FileWriter =

    // ── Global root resolution ────────────────────────────────────────────────

    let private isUnix () =
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)

    /// Returns the OS-correct global config root for a harness.
    /// Raises NotSupportedException for Copilot (no global file-system scope).
    let resolveGlobalRoot (harness: AgentHarness) : string =
        match harness with
        | AgentHarness.Copilot ->
            raise (NotSupportedException "Copilot does not support global file-system scope")
        | AgentHarness.Opencode ->
            if isUnix () then
                let home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                Path.Combine(home, ".config", "opencode")
            else
                let appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                Path.Combine(appData, "opencode")
        | AgentHarness.ClaudeCode ->
            let home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            Path.Combine(home, ".claude")

    // ── Harness root directories ──────────────────────────────────────────────

    let private harnessProjectRoot (harness: AgentHarness) (rootDir: string) : string =
        match harness with
        | AgentHarness.Opencode  -> Path.Combine(rootDir, ".opencode")
        | AgentHarness.Copilot   -> Path.Combine(rootDir, ".github")
        | AgentHarness.ClaudeCode -> Path.Combine(rootDir, ".claude")

    let private scopeRoot (harness: AgentHarness) (scope: WriteScope) : string =
        match scope with
        | Project rootDir ->
            let absRoot = Path.GetFullPath(rootDir)
            harnessProjectRoot harness absRoot
        | Global ->
            resolveGlobalRoot harness   // raises for Copilot

    // ── Path resolution ───────────────────────────────────────────────────────

    /// Pure function: returns the absolute output path for an artifact.
    /// No I/O is performed.
    let resolveOutputPath
        (harness: AgentHarness)
        (kind: ArtifactKind)
        (name: string)
        (scope: WriteScope) : string =
        let root = scopeRoot harness scope
        match kind with
        | AgentArtifact ->
            let subdir =
                match harness with
                | AgentHarness.Copilot    -> "agents"
                | AgentHarness.Opencode   -> "agents"
                | AgentHarness.ClaudeCode -> "agents"
            Path.Combine(root, subdir, name + ".md")
        | SkillArtifact ->
            let subdir =
                match harness with
                | AgentHarness.Copilot    -> "skills"
                | AgentHarness.Opencode   -> "skills"
                | AgentHarness.ClaudeCode -> "skills"
            Path.Combine(root, subdir, name, "SKILL.md")
        | CommandArtifact ns ->
            match harness with
            | AgentHarness.Opencode ->
                Path.Combine(root, "commands", name + ".md")
            | AgentHarness.Copilot ->
                Path.Combine(root, "prompts", name + ".prompt.md")
            | AgentHarness.ClaudeCode ->
                match ns with
                | None           -> Path.Combine(root, "commands", name + ".md")
                | Some namespace_ -> Path.Combine(root, "commands", namespace_, name + ".md")

    // ── I/O layer ─────────────────────────────────────────────────────────────

    /// Writes content to the resolved path, creating intermediate directories as needed.
    /// Returns the resolved path.
    let writeFile
        (harness: AgentHarness)
        (kind: ArtifactKind)
        (name: string)
        (scope: WriteScope)
        (content: string) : string =
        let path = resolveOutputPath harness kind name scope
        Directory.CreateDirectory(Path.GetDirectoryName(path)) |> ignore
        File.WriteAllText(path, content)
        path

    // ── Convenience wrappers ──────────────────────────────────────────────────

    let private requireName (frontmatter: Map<string, obj>) (typeName: string) : string =
        match frontmatter |> Map.tryFind "name" with
        | Some v when not (String.IsNullOrWhiteSpace(string v)) -> string v
        | _ ->
            raise (ArgumentException(
                $"{typeName} requires a 'name' in frontmatter to derive the output filename."))

    /// Renders an Agent and writes it to the harness-correct path.
    /// Returns the resolved path.
    let writeAgent
        (agent: Agent)
        (harness: AgentHarness)
        (scope: WriteScope)
        (configure: AgentWriter.Options -> unit) : string =
        let name = requireName agent.Frontmatter "Agent"
        let content =
            AgentWriter.renderAgent agent (fun opts ->
                opts.OutputFormat <- harness
                configure opts)
        writeFile harness AgentArtifact name scope content

    /// Renders a Skill and writes it to the harness-correct path.
    /// Returns the resolved path.
    let writeSkill
        (skill: Skill)
        (harness: AgentHarness)
        (scope: WriteScope)
        (configure: AgentWriter.Options -> unit) : string =
        let name = requireName skill.Frontmatter "Skill"
        let content = AgentWriter.renderSkill skill harness configure
        writeFile harness SkillArtifact name scope content

    /// Renders a SlashCommand and writes it to the harness-correct path.
    /// The optional namespace_ is only used for ClaudeCode subdirectory commands.
    /// Returns the resolved path.
    let writeCommand
        (cmd: SlashCommand)
        (harness: AgentHarness)
        (scope: WriteScope)
        (namespace_: string option)
        (configure: AgentWriter.Options -> unit) : string =
        let content =
            AgentWriter.renderCommand cmd (fun opts ->
                opts.OutputFormat <- harness
                configure opts)
        writeFile harness (CommandArtifact namespace_) cmd.Name scope content
