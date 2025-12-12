## 1. Implementation
- [ ] 1.1 Create solution: `dotnet new sln -n FsAgent`
- [ ] 1.2 Create F# class library: `src/FsAgent` via `dotnet new classlib -lang F# -o src/FsAgent`
- [ ] 1.3 Create F# test project: `tests/FsAgent.Tests` via `dotnet new xunit -lang F# -o tests/FsAgent.Tests`
- [ ] 1.4 Add projects to solution: `dotnet sln add src/FsAgent/FsAgent.fsproj tests/FsAgent.Tests/FsAgent.Tests.fsproj`
- [ ] 1.5 Add project reference: `dotnet add tests/FsAgent.Tests/FsAgent.Tests.fsproj reference src/FsAgent/FsAgent.fsproj`
- [ ] 1.6 Add dependencies to library: `dotnet add src/FsAgent/FsAgent.fsproj package YamlDotNet` and `dotnet add src/FsAgent/FsAgent.fsproj package System.Text.Json`
- [ ] 1.7 Ensure documented folders exist: `docs/`, `knowledge/` (create if missing; no content needed)
- [ ] 1.8 Build: `dotnet build`
- [ ] 1.9 Test: `dotnet test`
- [ ] 1.10 Update `README.md` with build/test instructions if needed
- [ ] 1.11 Update `CHANGELOG.md` noting the initial solution setup

## 2. Validation
- [ ] 2.1 Clean checkout builds and tests pass
- [ ] 2.2 Packages restore without additional tooling
- [ ] 2.3 Solution structure matches `openspec/project.md` conventions
