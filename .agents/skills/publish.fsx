// Run `dotnet fsi .agents/skills/publish.fsx` from the repo root.
// Generates .agents/skills/publish/SKILL.md

#r "../../src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent.Skills
open FsAgent.Writers

let projectRoot =
    System.IO.Path.GetFullPath(
        System.IO.Path.Combine(__SOURCE_DIRECTORY__, "..", ".."))

let publishSkill =
    skill {
        name "publish"
        description "Guide for releasing a new version of FsAgent — changelog format, release script, and manual steps."
        license "MIT"
        metadata (Map.ofList [
            "author",  box "fsagent example"
            "version", box "1.0.0"
        ])

        section "Format" """CHANGELOG.md structure:

```
## [Unreleased]

### Added
- Feature description

### Changed
- Enhancement description

### Changed (Breaking)
- Breaking change description

### Fixed
- Bug fix description

### Removed
- Removed feature description
```"""

        section "Release Process" """```bash
dotnet build                    # Ensure build passes
dotnet test                     # Ensure tests pass
./publish.sh                    # Run release script - done by human or CI pipeline
```

The script prompts for major/minor/patch, updates version in `.fsproj`,
moves unreleased changes to a versioned section in CHANGELOG.md,
creates git commit and tag, optionally pushes.
Use `--dry-run` flag to preview changes."""

        section "Pre-Publish Checklist" """- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds (`dotnet build -c Release`)
- [ ] CHANGELOG.md has unreleased entries under `## [Unreleased]`
- [ ] Changes are small and focused
- [ ] No uncommitted changes in working tree"""

        section "Manual Steps" """```bash
./publish.sh
```"""
    }

let path =
    FileWriter.writeSkill publishSkill AgentWriter.Opencode (Project projectRoot) (fun _ -> ())
printfn "Wrote %s" path
