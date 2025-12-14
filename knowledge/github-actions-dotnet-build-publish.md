# GitHub Actions: Build and Publish (.NET 10)

This guide shows how to set up reliable GitHub Actions workflows to build, test, and publish .NET 10 projects. It uses `actions/setup-dotnet` v5, NuGet lock files for reproducible restores, and secure publishing to nuget.org.

References:
- Example build workflow (fennel): https://raw.githubusercontent.com/dburriss/fennel/refs/heads/master/.github/workflows/build.yml
- Example publish workflow (fennel): https://raw.githubusercontent.com/dburriss/fennel/refs/heads/master/.github/workflows/publish.yml
- Action: actions/setup-dotnet: https://github.com/actions/setup-dotnet
- NuGet lock files and locked mode: https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies


## Build & Test Workflow
Use this for CI on pull requests and pushes. Pins the .NET SDK to `10.0.x`, enables NuGet caching, and restores in locked mode when a `packages.lock.json` is present.

```
# .github/workflows/build.yml
name: Build & Test (.NET 10)

on:
  push:
    branches: [ "*" ]
  pull_request:
    branches: [ "*" ]

permissions:
  contents: read

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v5

      # Setup .NET 10 SDK
      - name: Setup .NET 10 SDK
        uses: actions/setup-dotnet@v5
        with:
          dotnet-version: '10.0.x'
          # Enable cache when packages.lock.json exists
          cache: true

      # Optional: fail fast if lock file missing
      # - run: |
      #     test -f ./packages.lock.json || (echo "Missing packages.lock.json" && exit 1)

      - name: Restore (locked-mode)
        run: dotnet restore --locked-mode

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Test
        run: dotnet test --configuration Release --no-build --verbosity normal
```

Notes:
- `dotnet-version: '10.0.x'` installs the latest 10.0 patch.
- Enabling `cache: true` uses NuGet package caching keyed off `packages.lock.json`.
- `--locked-mode` ensures reproducible restores using the lock file; see below for enabling lock files.


## Publish Workflow (nuget.org)
Use this when pushing tags or when merging to `main`/`master`. It builds, tests, packs, and pushes `.nupkg` to nuget.org using `NUGET_API_KEY` stored as a repository secret.

```
# .github/workflows/publish.yml
name: Publish (.NET 10)

on:
  push:
    tags:
      - 'v*'    # e.g., v1.2.3
  workflow_dispatch:

permissions:
  contents: read

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v5

      - name: Setup .NET 10 SDK
        uses: actions/setup-dotnet@v5
        with:
          dotnet-version: '10.0.x'
          cache: true

      - name: Restore (locked-mode)
        run: dotnet restore --locked-mode

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Test
        run: dotnet test --configuration Release --no-build --verbosity normal

      # Adjust path if your pack root differs (e.g., src/YourProject)
      - name: Pack
        run: dotnet pack --configuration Release --no-build --output ./artifacts

      - name: Publish to nuget.org
        env:
          NUGET_AUTH_TOKEN: ${{ secrets.NUGET_API_KEY }}
        run: dotnet nuget push ./artifacts/*.nupkg \
              --source https://api.nuget.org/v3/index.json \
              --api-key $NUGET_AUTH_TOKEN \
              --skip-duplicate
```

Recommendations:
- Trigger on tags for release versioning (`v1.2.3`).
- Use `--skip-duplicate` to make re-runs idempotent.
- Keep the `.nupkg` output in a dedicated folder like `./artifacts`.


## Enable NuGet Lock Files (Reproducible Restores)
NuGet lock files capture the full dependency graph. Check in lock files for applications and CI reproducibility.

1) Turn on lock file generation in your project:
```
# In your .csproj/.fsproj
<PropertyGroup>
  <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  <!-- Optional: enforce locked mode locally -->
  <RestoreLockedMode>true</RestoreLockedMode>
</PropertyGroup>
```

2) Create the lock file:
```
dotnet restore
# Generates packages.lock.json at the project root
```

3) CI uses locked mode:
- `dotnet restore --locked-mode` in your workflows.
- With `actions/setup-dotnet@v5` and `cache: true`, the action uses lock file hashing for cache keys.

What to commit:
- Applications: commit `packages.lock.json`.
- Libraries: generally do not commit lock files; downstream apps will resolve their own graphs.


## SDK Pinning (Optional)
To force a specific SDK version, add `global.json` in the repo root:
```
{
  "sdk": { "version": "10.0.402" }
}
```
`actions/setup-dotnet` also supports `global-json-file` when the file is not at root.


## Dotnet 10 Notes
- Package pruning is enabled by default on .NET 10 targets, reducing unnecessary packages in graphs.
- Prefer `--no-restore` and `--no-build` flags to keep steps fast and deterministic.
- Use `TreatWarningsAsErrors` and NuGet warning controls (`NoWarn`, `WarningsAsErrors`) for clean builds.


## Security & Secrets
- Store `NUGET_API_KEY` as a repository secret; never hardcode keys in workflows.
- Use minimal `permissions` in workflows (`contents: read` is sufficient for most builds).
- Consider adding signing steps if you require signed packages.


## Troubleshooting
- Missing lock file: disable `--locked-mode` or add/commit `packages.lock.json`.
- Version selection drift: add `global.json` or pin `dotnet-version` to exact SDK.
- Cache misses: verify `packages.lock.json` path; set `cache-dependency-path` for monorepos.


## Minimal Build Example (Monorepo)
If your solution sits in a subfolder:
```
- uses: actions/setup-dotnet@v5
  with:
    dotnet-version: '10.0.x'
    cache: true
    cache-dependency-path: src/YourProject/packages.lock.json
- run: dotnet restore --locked-mode src/YourProject/YourProject.fsproj
- run: dotnet build --configuration Release --no-restore src/YourProject/YourProject.fsproj
- run: dotnet test --configuration Release --no-build tests/YourProject.Tests/YourProject.Tests.fsproj
```
