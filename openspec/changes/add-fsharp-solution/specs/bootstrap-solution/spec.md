## ADDED Requirements

### Requirement: Initialize F# Solution
The system SHALL provide a minimal F# .NET solution scaffold using the documented folder layout and the `dotnet` CLI for generation and dependency management.

#### Scenario: Generate solution and base projects
- **WHEN** the setup tasks are executed
- **THEN** a solution file exists at the repository root
- **AND** an F# class library exists at `src/FsAgent`
- **AND** the library project is added to the solution

#### Scenario: Provide test project
- **WHEN** the setup tasks are executed
- **THEN** an F# xUnit test project exists at `tests/FsAgent.Tests`
- **AND** it references `src/FsAgent/FsAgent.fsproj`
- **AND** the test project is added to the solution

#### Scenario: Add required dependencies
- **WHEN** the setup tasks are executed
- **THEN** the library project includes `YamlDotNet` and `System.Text.Json` as package references

#### Scenario: Ensure documented directories
- **WHEN** the setup tasks are executed
- **THEN** the repository includes `docs/` and `knowledge/` directories (empty or existing)

#### Scenario: Build and test succeed
- **WHEN** `dotnet build` and `dotnet test` run on a clean checkout
- **THEN** the solution builds successfully
- **AND** all tests run (even if they are placeholders) without errors

#### Scenario: CLI-only bootstrap
- **WHEN** initializing the solution
- **THEN** only the `dotnet` CLI is required (no custom build tools)
