# Using CHANGELOG.md in FsAgent

Audience: AI agents working in this repository who need clear guidance on maintaining release notes.

Overview
- We maintain a human‑readable CHANGELOG.md to document notable changes between versions.
- Follow the Keep a Changelog convention and Semantic Versioning.
- Update the changelog continuously during development (Unreleased section) and finalize it at release time.

Local policy and workflow
- Update CHANGELOG.md whenever a feature is added, modified, or removed (per AGENTS.md).
- Track ongoing changes under an "Unreleased" section.
- At release time:
  - Move the Unreleased entries into a new version section titled `## [X.Y.Z] - YYYY-MM-DD` (ISO 8601 date).
  - Add a fresh `## [Unreleased]` section below.
  - Sync `<PackageReleaseNotes>` in `src/FsAgent/FsAgent.fsproj` with the release section.
  - Commit and tag using the publish script or docs:
    - Commit: `release: prepare vX.Y.Z`
    - Tag: `vX.Y.Z`, then push `main` and the tag.
  - The GitHub Actions workflow will build, test, and publish to NuGet.

Format conventions (Keep a Changelog)
- File name is `CHANGELOG.md`.
- Reverse chronological order: latest version first.
- Use an `Unreleased` section to collect upcoming changes.
- Group entries by type:
  - Added: new features
  - Changed: modifications to existing functionality
  - Deprecated: functionality to be removed in a future version
  - Removed: functionality removed in this version
  - Fixed: bug fixes
  - Security: vulnerabilities addressed
- Prefer concise, human‑oriented descriptions over raw commit messages.
- Include the release date (YYYY‑MM‑DD). Consider linking versions to tag or compare URLs when relevant.

Versioning (Semantic Versioning)
- MAJOR: incompatible API changes
- MINOR: backward‑compatible functionality additions or deprecations
- PATCH: backward‑compatible bug fixes
- Pre‑releases: `-alpha`, `-beta`, `-rc.1` etc. Build metadata: `+` suffix (ignored for precedence)

Writing good entries
- Be explicit about breaking changes and deprecations; make upgrade paths clear.
- Keep entries scoped to user‑visible impact (summarize across commits).
- Avoid noise (merge commits, refactors without impact, etc.).
- Use consistent language and tense; keep it short and scannable.
- If a release is yanked due to a serious issue, mark it with `[YANKED]` in the heading.

Example skeleton

```
# Changelog

## [Unreleased]

### Added
- ...

### Changed
- ...

### Deprecated
- ...

### Removed
- ...

### Fixed
- ...

### Security
- ...

## [0.2.0] - 2025-12-15

### Added
- Markdown writer with configurable options
- Agent DSL: `agent { ... }` CE for authoring

### Changed
- ...

### Fixed
- ...
```

Repo‑specific references
- CHANGELOG.md currently contains an `Unreleased` section with entries such as initial solution setup, dependencies, Markdown writer, Agent DSL, frontmatter helpers, and import inference.
- Publishing steps and prompts are codified in `docs/publishing.md` and `publish.sh`.

Common pitfalls
- Forgetting to move `Unreleased` entries into a dated version before tagging.
- Using commit logs verbatim; prefer curated, human‑oriented entries.
- Omitting dates or inconsistent headings.
- Not calling out deprecations or breaking changes (makes upgrades risky).

Sources
- Keep a Changelog: https://keepachangelog.com/en/1.1.0/
- Semantic Versioning 2.0.0: https://semver.org/spec/v2.0.0.html
- Local docs: `docs/publishing.md`, `publish.sh`, `AGENTS.md`