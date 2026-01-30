## 1. DSL Changes

- [ ] 1.1 Rename `Import` custom operation to `ImportRaw` in `AgentBuilder`
- [ ] 1.2 Add new `Import` custom operation that calls `AST.importRef` (same as `ImportRaw`)

## 2. MarkdownWriter Changes

- [ ] 2.1 Add `IncludeCodeBlock` case to `ImportInclusion` discriminated union
- [ ] 2.2 Add helper function to map `DataFormat` to language tag string (yaml/json/toon/empty)
- [ ] 2.3 Implement code-block wrapping in `writeNode` for `Imported` nodes when `IncludeCodeBlock` is set

## 3. Tests

- [ ] 3.1 Add test for `importRaw` DSL operation creating correct AST node
- [ ] 3.2 Add test for `import` DSL operation creating correct AST node
- [ ] 3.3 Add test for `IncludeCodeBlock` with JSON format wrapping content in ```json
- [ ] 3.4 Add test for `IncludeCodeBlock` with YAML format wrapping content in ```yaml
- [ ] 3.5 Add test for `IncludeCodeBlock` with TOON format wrapping content in ```toon
- [ ] 3.6 Add test for `IncludeCodeBlock` with Unknown format using plain ``` fence

## 4. Documentation

- [ ] 4.1 Update README example to show `import` and `importRaw` usage
- [ ] 4.2 Document breaking change in release notes (import → importRaw migration)
