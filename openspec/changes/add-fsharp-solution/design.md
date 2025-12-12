## Context
We are introducing an initial F# solution scaffold aligned with the project's stratified design and conventions. The setup uses the `dotnet` CLI to generate projects and add dependencies without adding custom build tooling at this stage.

## Goals / Non-Goals
- Goals:
  - Provide a minimal, working F# solution with tests
  - Use documented structure (`src/`, `tests/`, keep `docs/`, `knowledge/`)
  - Add essential dependencies (YamlDotNet, System.Text.Json)
  - Succeed with `dotnet build` and `dotnet test`
- Non-Goals:
  - Implement DSL/AST/Writers functionality
  - Add `fsx` build scripts or CI configuration
  - Introduce multi-project stratification beyond a single library initially

## Decisions
- Start with a single F# class library (`src/FsAgent`) to keep complexity low
- Add a single F# xUnit test project (`tests/FsAgent.Tests`) referencing the library
- Use `dotnet` CLI exclusively for scaffolding and package management
- Add `YamlDotNet` and `System.Text.Json` to the library project to match tech stack

## Alternatives considered
- Multi-project stratification (DSL, AST, Writers) from day one → deferred to future changes to avoid premature complexity
- Using FAKE or `fsx` scripts → deferred to keep initial setup simple and CLI-only

## Risks / Trade-offs
- Single-project start may delay strict layer boundaries → acceptable for bootstrap, revisit when functionality lands

## Migration Plan
- Create solution and projects
- Add dependencies and references
- Confirm build and tests pass
- Update minimal docs and changelog
