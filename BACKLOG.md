# Backlog

Tasks are grouped by functional area. Items marked `[breaking]` require a semver major bump. Items marked `[dogfood]` are about using the library to build first-party tooling rather than changing the library itself.

---

## Functional Area: DSL — Agent Builder

Gaps and enhancements to the `agent { ... }` computation expression.

- [ ] Review `template` and `templateFile` operations to `AgentBuilder` CE (currently only `Prompt`, `Command`, and `Skill` builders support these). Should these be added to agent, or removed from the others?
- [ ] Add `model` field to `CommandBuilder` CE — `NOTES.md` notes "command can take a model"
- [ ] Investigate and specify command parameters support; determine whether they are harness-dependent and how they should be represented in the AST

---

## Functional Area: AST — Type Safety

Improvements to the core intermediate representation.

- [ ] `[breaking]` Replace `Map<string, obj>` with `Map<string, JsonElement>` (or a purpose-built DU) for the `Frontmatter` field on `Agent`, `Prompt`, `Skill`, and `SlashCommand` to avoid lossiness during serialisation round-trips — plan for a major version bump (1.0.0)
- [ ] Review `fmList` and `fmMap` AST helpers for compatibility with the new frontmatter type once the above is decided

---

## Functional Area: Writers — Rendering

Enhancements and correctness fixes in `Writers.fs`.

- [ ] Implement TOON serialization in the import pipeline — currently `DataFormat.Toon` is recognised by `inferFormat` but no TOON serializer exists; writers fall back to raw `File.ReadAllText`. Check for existing TOON serializers that can be leveraged, or implement a custom one if needed.
- [ ] Implement YAML-to-TOON and JSON-to-TOON re-serialisation at write time (architecture calls for late-bound import formatting)
- [ ] Implement JSON-to-YAML and YAML-to-JSON cross-format re-serialisation in the import pipeline
- [ ] Verify and add tests for the `Imported` node `wrapInCodeBlock = false` path end-to-end

---

## Functional Area: File Writer — Path Resolution

Gaps in `FileWriter.fs` path resolution and scope support.

- [ ] Support skills being written to `.agents/` folder for harnesses that use that convention (currently `resolveOutputPath` for `SkillArtifact` does not vary by harness)
- [ ] Implement global Copilot config path resolution — currently raises `NotSupportedException`; NOTES.md calls for a config file for global Copilot location
- [ ] Expose a `ConfigPaths` module (or equivalent) with higher-level helpers for resolving harness-specific root directories, consistent with what `knowledge/fsagent-api.md` documents
- [ ] Consider introducing a file-IO abstraction (interface or function set) to decouple `FileWriter` from direct `File.*` / `Directory.*` calls, enabling in-memory testing without hitting the filesystem

---

## Functional Area: Agent Reader

Parsing existing agent markdown files back into the AST (currently write-only).

- [ ] Design the `AgentReader` API: decide on input type (file path vs. string), return type (Result vs. exception), and which frontmatter fields to map
- [ ] Implement frontmatter YAML parsing into `Map<string, JsonElement>` (depends on AST type-safety task above)
- [ ] Implement section parsing: map ATX headings to `Section` nodes
- [ ] Implement XML section style parsing (counterpart to the `Xml` `SectionStyle` writer)
- [ ] Add `C`-category tests for the reader (filesystem boundary, YAML parsing boundary)
- [ ] Add `A`-category round-trip tests: `agent → renderAgent → AgentReader.parse → agent` yields equivalent structure

---

## Functional Area: Testing

Coverage gaps and test-quality improvements.

- [ ] Add golden-file tests for `renderAgent` / `renderPrompt` / `renderCommand` / `renderSkill` output across all three harnesses (currently only structural assertions exist)
- [ ] Add `C`-category tests for the YAML import path once re-serialisation is implemented
- [ ] Add `C`-category tests for the JSON import path
- [ ] Add `C`-category tests for TOON import path once TOON serializer exists
- [ ] Add tests for `AgentBuilder` `template` / `templateFile` operations once implemented
- [ ] Add tests covering `model` field on commands once implemented

---

## Functional Area: Documentation

Keeping docs current with implementation.

- [ ] Update `README.md` to document `FileWriter` module, `SectionStyle` option, and `Skill` / `renderSkill` (these are in code but the README predates them)
- [ ] Update `knowledge/fsagent-api.md` to reflect actual implemented API (remove references to `toolReference` DSL operation which does not exist; update `ConfigPaths` section)
- [ ] Update `ARCHITECTURE.md` to note the deferred `IWriter` interface decision and the planned `AgentReader` addition

---

## Functional Area: Dogfooding — First-Party Agents and Skills

Using the library to build agents, skills, and commands for the project's own workflow. These are tracked separately from library tasks.

- [ ] `[dogfood]` Write a `code-improvement` agent definition using the library (for iterative refactoring and code review tasks)
- [ ] `[dogfood]` Write a `testing` agent definition for TDD assistance and test generation
- [ ] `[dogfood]` Write a skill for using the library to create agents (authoring guide + templated sections)
- [ ] `[dogfood]` Write a skill for using the library to create commands
- [ ] `[dogfood]` Write a skill for using the library to create skills
- [ ] `[dogfood]` Write slash commands to streamline planning and workflow (building on the existing `opsx-*` command set)
- [ ] `[dogfood]` Generate and commit first-party agent/skill/command files to `.opencode/agents/` using the library's `FileWriter`
