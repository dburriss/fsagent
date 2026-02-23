namespace FsAgent.Writers

open System
open System.IO
open System.Runtime.InteropServices
open FsAgent.Agents
open FsAgent.Skills
open FsAgent.Commands
open System.IO.Abstractions

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

/// Controls which root directory project-scope skills are written to.
/// Affects OpenCode for all cases; also affects Copilot for ClaudeFolder.
type FolderVariant =
    /// Write to `.agents/skills/` — the cross-tool Agent Skills spec path (default).
    | AgentsFolder
    /// Write to `.opencode/skills/` — the OpenCode-specific path.
    | OpencodeFolder
    /// Write to `.claude/skills/` — supported by ClaudeCode, OpenCode, and Copilot.
    | ClaudeFolder

/// Public module exposing pure path helpers for harness root directories.
module ConfigPaths =

    let private isUnix () =
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)

    /// Returns the harness-specific project root subdirectory within rootDir.
    /// The rootDir is resolved to an absolute path.
    let resolveProjectRoot (harness: AgentHarness) (rootDir: string) : string =
        let absRoot = Path.GetFullPath(rootDir)
        match harness with
        | AgentHarness.Opencode   -> Path.Combine(absRoot, ".opencode")
        | AgentHarness.Copilot    -> Path.Combine(absRoot, ".github")
        | AgentHarness.ClaudeCode -> Path.Combine(absRoot, ".claude")

    /// Returns the harness-specific global config root.
    /// For Copilot: explicit copilotRoot → COPILOT_GLOBAL_ROOT env var → NotSupportedException.
    /// For Opencode and ClaudeCode: resolved from OS profile.
    let resolveGlobalRoot (harness: AgentHarness) (copilotRoot: string option) : string =
        match harness with
        | AgentHarness.Copilot ->
            match copilotRoot with
            | Some r -> r
            | None ->
                let envVal = Environment.GetEnvironmentVariable("COPILOT_GLOBAL_ROOT")
                if not (String.IsNullOrEmpty(envVal)) then envVal
                else raise (NotSupportedException "Copilot does not support global file-system scope")
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

module FileWriter =

    // ── Internal helpers ──────────────────────────────────────────────────────

    let private scopeRoot (harness: AgentHarness) (scope: WriteScope) (copilotRoot: string option) : string =
        match scope with
        | Project rootDir ->
            ConfigPaths.resolveProjectRoot harness rootDir
        | Global ->
            ConfigPaths.resolveGlobalRoot harness copilotRoot

    // ── Path resolution ───────────────────────────────────────────────────────

    /// Pure function: returns the absolute output path for an artifact.
    /// Uses AgentsFolder as default for OpenCode project-scope skills.
    /// Equivalent to calling resolveOutputPathWith with AgentsFolder.
    /// No I/O is performed.
    let resolveOutputPath
        (harness: AgentHarness)
        (kind: ArtifactKind)
        (name: string)
        (scope: WriteScope) : string =
        let root = scopeRoot harness scope None
        match kind with
        | AgentArtifact ->
            Path.Combine(root, "agents", name + ".md")
        | SkillArtifact ->
            match harness, scope with
            | AgentHarness.Opencode, Project _ ->
                // Default: use cross-tool Agent Skills spec path (.agents/)
                let absRoot = Path.GetFullPath(
                    match scope with Project r -> r | Global -> "")
                Path.Combine(absRoot, ".agents", "skills", name, "SKILL.md")
            | _ ->
                Path.Combine(root, "skills", name, "SKILL.md")
        | CommandArtifact ns ->
            match harness with
            | AgentHarness.Opencode ->
                Path.Combine(root, "commands", name + ".md")
            | AgentHarness.Copilot ->
                Path.Combine(root, "prompts", name + ".prompt.md")
            | AgentHarness.ClaudeCode ->
                match ns with
                | None            -> Path.Combine(root, "commands", name + ".md")
                | Some namespace_ -> Path.Combine(root, "commands", namespace_, name + ".md")

    /// Pure function: returns the absolute output path for an artifact.
    /// Accepts an explicit FolderVariant to control project-scope skill root.
    /// ClaudeFolder affects both OpenCode and Copilot; AgentsFolder/OpencodeFolder affect OpenCode only.
    /// No I/O is performed.
    let resolveOutputPathWith
        (harness: AgentHarness)
        (kind: ArtifactKind)
        (name: string)
        (scope: WriteScope)
        (folderVariant: FolderVariant) : string =
        let root = scopeRoot harness scope None
        match kind with
        | AgentArtifact ->
            Path.Combine(root, "agents", name + ".md")
        | SkillArtifact ->
            match harness, scope with
            | (AgentHarness.Opencode | AgentHarness.Copilot), Project _ when folderVariant = ClaudeFolder ->
                let absRoot = Path.GetFullPath(
                    match scope with Project r -> r | Global -> "")
                Path.Combine(absRoot, ".claude", "skills", name, "SKILL.md")
            | AgentHarness.Opencode, Project _ ->
                let absRoot = Path.GetFullPath(
                    match scope with Project r -> r | Global -> "")
                let skillsRoot =
                    match folderVariant with
                    | AgentsFolder   -> Path.Combine(absRoot, ".agents")
                    | OpencodeFolder -> Path.Combine(absRoot, ".opencode")
                    | ClaudeFolder   -> Path.Combine(absRoot, ".claude")  // unreachable; handled above
                Path.Combine(skillsRoot, "skills", name, "SKILL.md")
            | _ ->
                Path.Combine(root, "skills", name, "SKILL.md")
        | CommandArtifact ns ->
            match harness with
            | AgentHarness.Opencode ->
                Path.Combine(root, "commands", name + ".md")
            | AgentHarness.Copilot ->
                Path.Combine(root, "prompts", name + ".prompt.md")
            | AgentHarness.ClaudeCode ->
                match ns with
                | None            -> Path.Combine(root, "commands", name + ".md")
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

// ── AgentFileWriter class ─────────────────────────────────────────────────────

/// Injectable file writer that uses an IFileSystem abstraction for testable I/O.
/// Accepts a MockFileSystem (Testably.Abstractions.Testing) for in-memory testing.
type AgentFileWriter
    (fileSystem   : IFileSystem,
     scope        : WriteScope,
     ?configure   : AgentWriter.Options -> unit,
     ?copilotRoot : string,
     ?folderVariant : FolderVariant) =

    let configure_ = defaultArg configure (fun _ -> ())
    let folderVariant_ = defaultArg folderVariant AgentsFolder
    let copilotRoot_ = copilotRoot

    let scopeRoot (harness: AgentHarness) =
        match scope with
        | Project rootDir ->
            ConfigPaths.resolveProjectRoot harness rootDir
        | Global ->
            ConfigPaths.resolveGlobalRoot harness copilotRoot_

    let resolvePath (harness: AgentHarness) (kind: ArtifactKind) (name: string) =
        let root = scopeRoot harness
        match kind with
        | AgentArtifact ->
            Path.Combine(root, "agents", name + ".md")
        | SkillArtifact ->
            match harness, scope with
            | (AgentHarness.Opencode | AgentHarness.Copilot), Project _ when folderVariant_ = ClaudeFolder ->
                let absRoot = Path.GetFullPath(
                    match scope with Project r -> r | Global -> "")
                Path.Combine(absRoot, ".claude", "skills", name, "SKILL.md")
            | AgentHarness.Opencode, Project _ ->
                let absRoot = Path.GetFullPath(
                    match scope with Project r -> r | Global -> "")
                let skillsRoot =
                    match folderVariant_ with
                    | AgentsFolder   -> Path.Combine(absRoot, ".agents")
                    | OpencodeFolder -> Path.Combine(absRoot, ".opencode")
                    | ClaudeFolder   -> Path.Combine(absRoot, ".claude")  // unreachable; handled above
                Path.Combine(skillsRoot, "skills", name, "SKILL.md")
            | _ ->
                Path.Combine(root, "skills", name, "SKILL.md")
        | CommandArtifact ns ->
            match harness with
            | AgentHarness.Opencode ->
                Path.Combine(root, "commands", name + ".md")
            | AgentHarness.Copilot ->
                Path.Combine(root, "prompts", name + ".prompt.md")
            | AgentHarness.ClaudeCode ->
                match ns with
                | None            -> Path.Combine(root, "commands", name + ".md")
                | Some namespace_ -> Path.Combine(root, "commands", namespace_, name + ".md")

    let requireName (frontmatter: Map<string, obj>) (typeName: string) : string =
        match frontmatter |> Map.tryFind "name" with
        | Some v when not (String.IsNullOrWhiteSpace(string v)) -> string v
        | _ ->
            raise (ArgumentException(
                $"{typeName} requires a 'name' in frontmatter to derive the output filename."))

    let writeContent (path: string) (content: string) =
        let dir = Path.GetDirectoryName(path)
        fileSystem.Directory.CreateDirectory(dir) |> ignore
        fileSystem.File.WriteAllText(path, content)
        path

    /// Renders an Agent and writes it to the harness-correct path via the injected filesystem.
    /// Returns the resolved path.
    member _.WriteAgent(agent: Agent, harness: AgentHarness) : string =
        let name = requireName agent.Frontmatter "Agent"
        let content =
            AgentWriter.renderAgent agent (fun opts ->
                opts.OutputFormat <- harness
                configure_ opts)
        let path = resolvePath harness AgentArtifact name
        writeContent path content

    /// Renders a Skill and writes it to the harness-correct path via the injected filesystem.
    /// Returns the resolved path.
    member _.WriteSkill(skill: Skill, harness: AgentHarness) : string =
        let name = requireName skill.Frontmatter "Skill"
        let content = AgentWriter.renderSkill skill harness configure_
        let path = resolvePath harness SkillArtifact name
        writeContent path content

    /// Renders a SlashCommand and writes it to the harness-correct path via the injected filesystem.
    /// Returns the resolved path.
    member _.WriteCommand(cmd: SlashCommand, harness: AgentHarness, ?ns: string) : string =
        let content =
            AgentWriter.renderCommand cmd (fun opts ->
                opts.OutputFormat <- harness
                configure_ opts)
        let path = resolvePath harness (CommandArtifact ns) cmd.Name
        writeContent path content
