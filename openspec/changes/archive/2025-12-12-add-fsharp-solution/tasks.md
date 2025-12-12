## 1. Implementation
- [x] 1.1 Create solution: `dotnet new sln -n FsAgent`
- [x] 1.2 Create F# class library: `src/FsAgent` via `dotnet new classlib -lang F# -o src/FsAgent`
- [x] 1.3 Create F# test project: `tests/FsAgent.Tests` via `dotnet new xunit -lang F# -o tests/FsAgent.Tests`
- [x] 1.4 Add projects to solution: `dotnet sln add src/FsAgent/FsAgent.fsproj tests/FsAgent.Tests/FsAgent.Tests.fsproj`
- [x] 1.5 Add project reference: `dotnet add tests/FsAgent.Tests/FsAgent.Tests.fsproj reference src/FsAgent/FsAgent.fsproj`
- [x] 1.6 Add dependencies to library: `dotnet add src/FsAgent/FsAgent.fsproj package YamlDotNet` and `dotnet add src/FsAgent/FsAgent.fsproj package System.Text.Json`
- [x] 1.7 Ensure documented folders exist: `docs/`, `knowledge/` (create if missing; no content needed)
- [x] 1.8 Build: `dotnet build`
- [x] 1.9 Test: `dotnet test`
- [x] 1.10 Update `README.md` with build/test instructions if needed
- [x] 1.11 Update `CHANGELOG.md` noting the initial solution setup

## 2. Validation
- [x] 2.1 Clean checkout builds and tests pass
- [x] 2.2 Packages restore without additional tooling
- [x] 2.3 Solution structure matches `openspec/project.md` conventions
