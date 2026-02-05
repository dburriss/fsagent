# NuGet Lock Files and FSharp.Core Hash Mismatch Issue

## Problem

Local builds and GitHub Actions CI were failing with `NU1403` errors when using NuGet lock files with `RestoreLockedMode=true`:

```
error NU1403: Package content hash validation failed for FSharp.Core.10.0.102.
The package is different than the last restore.
```

The issue occurred in both environments:
- **Local development**: Failed when lock files contained hashes from GitHub Actions
- **GitHub Actions CI**: Failed when lock files contained hashes from local SDK

## Root Cause

Different .NET SDK installations bundle FSharp.Core with **different content hashes**, even for the same version number:

1. **Local SDK** (installed via mise): FSharp.Core 10.0.102 with hash `v91pt3B1oh...`
2. **GitHub Actions SDK**: FSharp.Core 10.0.102 with hash `dk/Rm4/s7e+...`
3. **NuGet.org package**: FSharp.Core 8.0.400 with hash `kHMdDDmlZl98...`

When F# projects don't explicitly reference FSharp.Core, the .NET SDK provides an **implicit reference** to its bundled version. This implicit reference has different hashes depending on where the SDK was obtained.

NuGet lock files (`packages.lock.json`) store content hashes for all packages. When `RestorePackagesWithLockFile=true`, NuGet validates these hashes during restore - even with `RestoreLockedMode=false`.

## Failed Attempts

### Attempt 1: Conditional RestoreLockedMode
```xml
<!-- Directory.Build.props -->
<RestoreLockedMode Condition="'$(CI)' == 'true'">true</RestoreLockedMode>
<RestoreLockedMode Condition="'$(CI)' != 'true'">false</RestoreLockedMode>
```

**Result**: Failed - NuGet still validates hashes when `RestorePackagesWithLockFile=true`, regardless of `RestoreLockedMode` setting.

### Attempt 2: Conditional RestorePackagesWithLockFile
```xml
<RestorePackagesWithLockFile Condition="'$(CI)' == 'true'">true</RestorePackagesWithLockFile>
<RestorePackagesWithLockFile Condition="'$(CI)' != 'true'">false</RestorePackagesWithLockFile>
```

**Result**: Failed - NuGet throws `NU1005` error when `RestorePackagesWithLockFile=false` but lock files exist in the repository.

### Attempt 3: Force NuGet.org Source
```bash
dotnet restore --source https://api.nuget.org/v3/index.json --force-evaluate
```

**Result**: Failed - SDK implicit reference still takes precedence over explicit source specification.

## Solution

Force both local and CI environments to use **the same FSharp.Core package from NuGet.org** by:

1. **Creating a repository-specific `nuget.config`** that only uses NuGet.org:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

2. **Explicitly referencing FSharp.Core** in all F# projects:

```xml
<ItemGroup>
  <!-- Explicitly reference FSharp.Core to force NuGet.org download instead of SDK bundle -->
  <PackageReference Include="FSharp.Core" Version="8.0.400" />
  <!-- other packages -->
</ItemGroup>
```

3. **Disabling the implicit SDK reference** in all F# projects:

```xml
<PropertyGroup>
  <!-- Disable implicit FSharp.Core from SDK to use explicit NuGet package -->
  <DisableImplicitFSharpCoreReference>true</DisableImplicitFSharpCoreReference>
</PropertyGroup>
```

4. **Regenerating lock files** with the NuGet.org FSharp.Core hash:

```bash
dotnet nuget locals all --clear
rm -f src/FsAgent/packages.lock.json tests/FsAgent.Tests/packages.lock.json
dotnet restore
```

## Files Changed

- **`nuget.config`** (created) - Forces nuget.org as only package source
- **`Directory.Build.props`** (created) - Conditional `RestoreLockedMode` based on CI environment
- **`src/FsAgent/FsAgent.fsproj`**:
  - Added `DisableImplicitFSharpCoreReference=true`
  - Added explicit `FSharp.Core` PackageReference
  - Removed hardcoded `RestoreLockedMode=true`
- **`tests/FsAgent.Tests/FsAgent.Tests.fsproj`**:
  - Added `DisableImplicitFSharpCoreReference=true`
  - Added explicit `FSharp.Core` PackageReference
- **`src/FsAgent/packages.lock.json`** - Regenerated with NuGet.org hash
- **`tests/FsAgent.Tests/packages.lock.json`** - Regenerated with NuGet.org hash

## Verification

### Local Development
```bash
dotnet clean
dotnet restore  # No NU1403 errors
dotnet build
dotnet test     # All 98 tests pass
```

### GitHub Actions CI
```bash
dotnet restore --locked-mode  # Passes hash validation
dotnet build
dotnet test
```

Both environments now use `FSharp.Core 8.0.400` from NuGet.org with hash `kHMdDDmlZl98tujgHCmL8/HNH9VKbxsRMC9s7wbwr4noR40SSa5D4d00yF8cMK52s8jabVuiLLcrUw9r+PkKDQ==`.

## Key Learnings

1. **Implicit SDK references** can cause hash mismatches across environments
2. **`RestoreLockedMode=false` doesn't disable hash validation** - it only allows lock file updates
3. **Repository-specific `nuget.config`** overrides global NuGet configuration
4. **`DisableImplicitFSharpCoreReference`** is required to prevent SDK from providing FSharp.Core
5. **Explicit PackageReferences** ensure consistent package sources across environments

## References

- [NuGet Lock Files Documentation](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NU1403 Error](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu1403)
- [GitHub Actions Environment Variables](https://docs.github.com/en/actions/reference/environment-variables)
- Related commits:
  - `32532b0` - Initial RestoreLockedMode fix attempt
  - `c9bce13` - Final solution with explicit FSharp.Core reference
