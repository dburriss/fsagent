<!-- OPENSPEC:START -->
# OpenSpec Instructions

These instructions are for AI assistants working in this project.

Always open `@/openspec/AGENTS.md` when the request:
- Mentions planning or proposals (words like proposal, spec, change, plan)
- Introduces new capabilities, breaking changes, architecture shifts, or big performance/security work
- Sounds ambiguous and you need the authoritative spec before coding

Use `@/openspec/AGENTS.md` to learn:
- How to create and apply change proposals
- Spec format and conventions
- Project structure and guidelines

Keep this managed block so 'openspec update' can refresh the instructions.

<!-- OPENSPEC:END -->

Key files:
- `README.md`
- `ARCHITECTURE.md`
- `opencode/project.md`

# General
- Keep answers succinct and information-dense.  
- Be critical of ideas; call out flawed assumptions, missing information, or unnecessary complexity.  
- Prefer simplicity and incremental changes.  
- State uncertainty explicitly when information is incomplete.  
- Work in small steps and verify each change.  
- When building new things, consult the @ARCHITECTURE.md file

# Tech Stack
- F# (.NET 10.0) with computation expressions for DSL
- YAML/JSON parsing (YamlDotNet, System.Text.Json)
- Custom TOON serializer for embedded data
- Optional testing with xUnit3
- Build automation with `dotnet` CLI and `fsx` scripts

# Build & Test
```bash
dotnet build                    # Build project
dotnet test                     # Run unit tests
fsx scripts/build.fsx           # Alternative build script
```

## Testing Guidelines
- Default to TDD.
- Prefer in-memory tests.
- Run build and tests before and after modifications.
- Use Assert.Fail("message") instead of Assert.True(false, "message") for test failures.

### ABC Test Categories
**A – Acceptance Tests**  
Validate agent generation and writer output. Test DSL→AST→Writer pipeline end-to-end with fast, in-memory execution.

**B – Building Tests**  
Temporary scaffolding during TDD, debugging, or exploration. These tests are disposable.

**C – Communication Tests**  
Validate external boundaries: YAML/JSON parsing, TOON serialization, file I/O for imports, writer frontmatter generation. Keep separate to avoid polluting acceptance tests.

Label tests by external dependencies (e.g., `yaml`, `json`, `filesystem`, `serialization`) to make execution constraints explicit.

# Repository Overview
An F# DSL and library for generating custom agent files for AI agent tools. The project uses stratified design: DSL layer builds AST, writers convert AST to output formats (Markdown, JSON), and low-level import pipeline handles file parsing and serialization.

Key directories:
- `/src` - Root project files (DSL, AST, Writers, Serialization modules)
- `/tests` - Test projects follow ABC style
- `/docs` - Basic docs for developers
- `/knowledge` - Summaries for compacting knowledge for AGENTS
- `.opencode/` - OpenCode plugin configuration and command definitions
- `openspec/` - Project specifications and change proposals

# Coding Guidelines
- Prefer simplicity.
- Use pure functions; push I/O to the boundaries.
- Apply functional programming where practical.
- Use stratified design: assemble complex behaviour from smaller, simpler components.
- DSL layer must not depend on output formats.
- Writers must not mutate AST.
- Follow naming: `PascalCase` for types/modules, `camelCase` for values/parameters.
- Run `build` and `test` before and after all code/config changes.
- Update `CHANGELOG.md` with changes when a feature is added, modified, or removed.

# Git Conventions
- `main` contains releasable code
- Feature branches: `feat/*`, `fix/*`, `refactor/*`
- Small, atomic commits with clear intent
- Commit pattern: `type: description` (e.g., `feat: add JSON writer`)
- No worktrees detected - use standard branch workflow
