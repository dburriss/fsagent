# Authoring and Publishing a NuGet Package (F#/.NET)

This guide summarizes NuGet best practices and the dotnet CLI workflow to package and publish a library you authored. It also shows a modernized `.fsproj` snippet inspired by your Fennel example.

## Prerequisites
- .NET SDK (SDK-style project)
- NuGet.org account + API key
- Project builds clean in `Release`

## Project Metadata (Required + Recommended)
Add these properties to your `.fsproj` under a single `<PropertyGroup>`.

Required/minimal:
- `PackageId` — globally unique identifier
- `Version` — SemVer: `Major.Minor.Patch[-prerelease]`
- `Authors` — your display name or org
- `Description` — concise summary

Highly recommended:
- `PackageProjectUrl` — project homepage or repo
- `RepositoryUrl` and `RepositoryType` (`git`) — source metadata
- `PackageLicenseExpression` — SPDX ID (for open source; e.g., `MIT`)
- `PackageReadmeFile` — `README.md` included in the package page
- `PackageIcon` — PNG icon (prefer 128×128, transparent)
- `PackageTags` — space-delimited tags
- `PackageReleaseNotes` — human-readable changes per release
- `GenerateDocumentationFile` — XML docs for API surface
- `PublishRepositoryUrl` — enable Source Link

Optional but useful:
- `RepositoryBranch` and `RepositoryCommit` — for reproducibility
- `GeneratePackageOnBuild` — pack on build (trade-off: slower builds)
- `IncludeSymbols` + `SymbolPackageFormat` — produce `.snupkg` for debugging
- `TargetFrameworks` — multi-target to reach more consumers

## Modernized `.fsproj` Example (F#)
This example follows current NuGet guidance. Adjust values for your project.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Target one or more frameworks -->
    <TargetFrameworks>net8.0;netstandard2.0</TargetFrameworks>

    <!-- Core package identity -->
    <PackageId>Contoso.MyLibrary</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>A concise description of the library.</Description>
    <Title>Contoso MyLibrary</Title>

    <!-- Links & source metadata -->
    <PackageProjectUrl>https://github.com/you/your-repo</PackageProjectUrl>
    <RepositoryUrl>https://github.com/you/your-repo</RepositoryUrl>
    <RepositoryType>git</RepositoryType>

    <!-- Licensing (prefer SPDX expression) -->
    <PackageLicenseExpression>MIT</PackageLicenseExpression>

    <!-- README and icon on NuGet.org -->
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageIcon>icon.png</PackageIcon>

    <!-- Discoverability & docs -->
    <PackageTags>fsharp library utility</PackageTags>
    <PackageReleaseNotes>Initial stable release.</PackageReleaseNotes>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>

    <!-- SourceLink / reproducible builds -->
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
    <EmbedUntrackedSources>true</EmbedUntrackedSources>

    <!-- Optional: pack on build -->
    <!-- <GeneratePackageOnBuild>true</GeneratePackageOnBuild> -->

    <!-- Optional: symbols package -->
    <!-- <IncludeSymbols>true</IncludeSymbols> -->
    <!-- <SymbolPackageFormat>snupkg</SymbolPackageFormat> -->
  </PropertyGroup>

  <ItemGroup>
    <!-- SourceLink provider for GitHub (private asset) -->
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All" />
  </ItemGroup>
</Project>
```

Notes:
- If `README.md` or `icon.png` are not at the project root, include them explicitly:

```xml
<ItemGroup>
  <None Include="docs/README.md" Pack="true" PackagePath="/" />
  <None Include="assets/icon.png" Pack="true" PackagePath="/" />
</ItemGroup>
```

## dotnet CLI Workflow
1. Build in Release:
   - `dotnet build -c Release`
2. Pack the package:
   - `dotnet pack -c Release`
   - Output: `bin/Release/<PackageId>.<Version>.nupkg`
3. (Optional) Create symbol package:
   - `dotnet pack -c Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg`
4. Test install locally:
   - Create a throwaway app and add your package from local file or a test feed
   - If you repack with same version, clear caches before retesting
     - `dotnet nuget locals all --clear`
5. Publish to NuGet.org:
   - `dotnet nuget push bin/Release/*.nupkg --source https://api.nuget.org/v3/index.json --api-key $NUGET_API_KEY`
   - For private feeds, set `--source` to your feed URL

## Best Practices (Summary)
- Use SDK-style projects and pack with `dotnet pack`.
- Choose a unique `PackageId`; follow namespace-like naming.
- Use SemVer; mark previews with `-prerelease`.
- Prefer `PackageLicenseExpression` (SPDX) over legacy URL fields.
- Provide `README` and `Icon` for better UX on NuGet.org.
- Add `RepositoryUrl`/`RepositoryType` and enable Source Link.
- Include meaningful `PackageTags` and `PackageReleaseNotes`.
- Target multiple frameworks when practical to broaden compatibility.
- Avoid exact-version dependencies unless truly required.
- Keep dependencies minimal; avoid shipping extraneous files.

## Updating Your Older Example (Fennel)
The Fennel project used a `License` URL. Modern guidance is to replace it with a license expression or license file:

```xml
<!-- Replace this (deprecated style): -->
<!-- <License>https://github.com/dburriss/fennel/blob/master/LICENSE</License> -->

<!-- With one of these: -->
<PackageLicenseExpression>MIT</PackageLicenseExpression>
<!-- or -->
<!-- <PackageLicenseFile>LICENSE</PackageLicenseFile> -->
```

Also consider adding:
- `PackageReadmeFile` and `PackageIcon`
- `RepositoryUrl` and `RepositoryType`
- `PublishRepositoryUrl` and SourceLink provider
- `GenerateDocumentationFile`

## Troubleshooting
- Package didn’t show README/icon: ensure files exist and are referenced; include via `<None Pack="true" PackagePath="/" />` if not at root.
- Repacking doesn’t update: bump `Version` or clear locals (`dotnet nuget locals all --clear`).
- Symbols not available: publish `.snupkg` to a symbols source; NuGet.org supports symbols via `dotnet nuget push` with `.snupkg`.
- Project not packable: set `<IsPackable>true</IsPackable>` or ensure SDK-style format.

## References
- Package authoring best practices — https://learn.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices
- Create a package (dotnet CLI) — https://learn.microsoft.com/en-us/nuget/create-packages/creating-a-package-dotnet-cli
