# Project Context

## Purpose

This project provides a small, composable F# DSL for generating agent markdown files. It enables defining agents once and rendering them into multiple ecosystem formats (OpenCode, Copilot, Claude, raw Markdown). The DSL resembles Giraffe.ViewEngine in structure and composability. Core goals: clean stratified design, flavour-agnostic agent definitions, pluggable writers, and the ability to import external files and re-serialize them (TOON, YAML, JSON) at write time.

## Tech Stack

* F# (net10.0)
* F# computation expressions for the DSL
* YAML/JSON parsing (YamlDotNet, System.Text.Json)
* Custom TOON serializer
* Optional testing with xUnit3
* Optional build automation with `dotnet` CLI and `fsx` scripts

## Project Conventions

### Code Style

* Prefer explicit types on public APIs.
* Internal AST types kept small and immutable.
* No hidden side-effects; all I/O happens in import utilities or writers.
* Naming conventions:

  * `PascalCase` for types and modules.
  * `camelCase` for private values and parameters.
  * DSL operations use lowercase functions (`role`, `objective`, `import`, `section`).
* Avoid deeply nested modules; group by layer (DSL, AST, Writers, Serialization).

### Architecture Patterns

* **Stratified Design:**

  * DSL is the top abstraction.
  * AST is the intermediate representation.
  * Writers are also a top level abstraction, meant to be past an agent template.
  * Writers/serializers are the lowest level.
* **AST-Driven Rendering:** one in-memory model; all writers operate on it.
* **Composable Node Structure:** Giraffe.ViewEngine-inspired `Node` tree.
* **Pluggable Writers:** Writers wrap the baseline Markdown writer and inject flavour-specific frontmatter rules.
* **Late-Bound Import Formatting:** AST carries import references (path + declared format); writers read those files via lower-level modules and choose the serialization format (TOON/YAML/JSON) at write time.
* **Pure Construction:** DSL builds pure data; no side-effects.
* **Small Surface Area:** API should stay minimal, orthogonal, and predictable.

### Testing Strategy

* Unit tests for:

  * AST construction (deterministic node shapes).
  * DSL combinators.
  * Writer behaviour (frontmatter expansion, import formatting).
  * Import pipeline: file → parsed object → reserialization.
* Golden-file tests for rendered Markdown.
* No integration tests unless necessary; project is mostly pure logic. Exception is file read and write.

### Git Workflow

* `main` contains releasable code.
* Feature branches use short-lived branches: `feat/*`, `fix/*`, `refactor/*`.
* Prefer small, atomic commits with clear intent.
* Commit messages follow a simple pattern:

  * `feat: add new section builder`
  * `fix: correct TOON serialization`
  * `docs: update project.md`

## Domain Context

* “Agent files” refer to Markdown documents containing frontmatter metadata and structured sections (`Role`, `Objective`, `Instructions`, etc.).
* “Flavours” (OpenCode, Copilot, Claude) represent different conventions in frontmatter and layout.
* Imported data may originate from YAML, JSON, or arbitrary formats and must be re-emittable as fenced blocks in other formats.
* TOON is a serialization format used for embedding structured data inside markdown.

## Important Constraints

* DSL must not depend on any specific output format.
* Writers must not mutate the AST.
* Imported data is referenced by path in the AST; writers resolve and parse via lower-level modules and select re-serialization format at write time.
* No flavour-specific logic allowed in the DSL layer.
* Rendering must be deterministic.
* Avoid introducing complexity that prevents manual extension by end-users.

## External Dependencies

* YAML parsing (YamlDotNet).
* JSON parsing (System.Text.Json).
* Optional file system access for imports.
* No runtime network dependencies.

