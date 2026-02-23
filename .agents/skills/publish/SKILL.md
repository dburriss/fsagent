---
description: Guide for releasing a new version of FsAgent — changelog format, release script, and manual steps.
license: MIT
metadata: 
  author: fsagent example
  version: 1.0.0
name: publish
---

# Format

CHANGELOG.md structure:

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
```

# Release Process

```bash
dotnet build                    # Ensure build passes
dotnet test                     # Ensure tests pass
./publish.sh                    # Run release script - done by human or CI pipeline
```

The script prompts for major/minor/patch, updates version in `.fsproj`,
moves unreleased changes to a versioned section in CHANGELOG.md,
creates git commit and tag, optionally pushes.
Use `--dry-run` flag to preview changes.

# Pre-Publish Checklist

- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds (`dotnet build -c Release`)
- [ ] CHANGELOG.md has unreleased entries under `## [Unreleased]`
- [ ] Changes are small and focused
- [ ] No uncommitted changes in working tree

# Manual Steps

```bash
./publish.sh
```
