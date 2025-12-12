# Change: Add F# solution setup

## Why
Set up a new F# project using the documented folder layout and the `dotnet` CLI. This enables building the DSL library with tests and dependencies aligned to the project conventions.

## What Changes
- Scaffold a .NET solution with an initial F# class library and test project
- Use `dotnet` CLI to generate projects and add dependencies
- Add essential packages: `YamlDotNet`, `System.Text.Json`
- Wire solution and project references to match documented structure
- Ensure `dotnet build` and `dotnet test` succeed on a clean checkout
- Update minimal docs to reflect build/test steps (no functional code changes)

## Impact
- Affected specs: `bootstrap-solution`
- Affected code: `src/`, `tests/`, `.sln` file, package references
- Tooling: `dotnet` CLI is the only required tool; optional `fsx` scripts remain out of scope for initial setup
