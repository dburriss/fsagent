# Spec: sectionFrom DSL Operation

## Purpose

Defines the `sectionFrom` custom operation available in all four CE builders (Prompt, Agent, Skill, Command), which produces a `Section` node containing an `Imported` node derived from a file path.

## Requirements

### Requirement: sectionFrom DSL operation
All four CE builders (Prompt, Agent, Skill, Command) SHALL expose a `sectionFrom` custom operation that accepts a section name and a file path, and produces a `Section(name, [Imported(path, inferFormat path, false)])` node.

#### Scenario: sectionFrom produces correct AST node
- **WHEN** `sectionFrom "Build & Test" "knowledge/agents/build-and-test.md"` is called in a builder
- **THEN** the resulting sections list contains `Section("Build & Test", [Imported("knowledge/agents/build-and-test.md", Markdown, false)])`

#### Scenario: sectionFrom renders section heading and file content
- **WHEN** a `Section` containing an `Imported(path, Markdown, false)` node is rendered by the Markdown writer
- **THEN** the output contains the section heading followed by the raw file content without a code block wrapper

#### Scenario: sectionFrom is available in all four builders
- **WHEN** `sectionFrom` is invoked inside a `prompt { }`, `agent { }`, `skill { }`, and `command { }` CE
- **THEN** each CE compiles and the resulting sections list contains the expected `Section` node

#### Scenario: sectionFrom missing file emits error marker
- **WHEN** `sectionFrom` references a path that does not exist at write time
- **THEN** the writer emits `[Error loading <path>]` in place of the section content (consistent with `import`/`importRaw` behavior)
