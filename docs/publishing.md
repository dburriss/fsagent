# Publishing Guide

This guide outlines the steps to publish a new version of FsAgent to NuGet.org.

## Prerequisites

- Ensure all changes are committed and pushed to `main`.
- Run `dotnet build` and `dotnet test` locally to verify everything works.

## Automated Publishing (Recommended)

You can use the `publish.sh` script to automate the versioning and tagging process.

```bash
# Dry run to preview changes
./publish.sh --dry-run

# Interactive release process
./publish.sh
```

This script will:
1. Parse the current version and changelog.
2. Prompt you to select the next version (Major, Minor, or Patch).
3. Update `src/FsAgent/FsAgent.fsproj` and `CHANGELOG.md` automatically.
4. Perform the git commit and tag operations.
5. Push changes to origin (after confirmation).

## Manual Steps

If you prefer to do it manually:

1. **Update CHANGELOG.md**
   - Move the `[Unreleased]` section to a new version section, e.g., `## [0.2.0] - YYYY-MM-DD`
   - Add a new `[Unreleased]` section below it.

2. **Update src/FsAgent/FsAgent.fsproj**
   - Increment the `<Version>` element (e.g., from `0.1.0` to `0.2.0`).
   - Update `<PackageReleaseNotes>` with the release notes from CHANGELOG.md.

3. **Commit the changes**
   - Commit with a message like `release: prepare v0.2.0`

4. **Create and push a tag**
   - Tag the commit: `git tag v0.2.0`
   - Push the tag: `git push origin v0.2.0`

The GitHub Actions workflow (`.github/workflows/publish.yml`) will automatically trigger on the tag push, build, test, pack, and publish the package to NuGet.org.

## Notes

- The workflow uses the version from the `.fsproj` file for packaging.
- Ensure `NUGET_API_KEY` is set as a repository secret for publishing.
- Publishing is idempotent due to `--skip-duplicate` flag.</content>
<filePath">docs/publishing.md