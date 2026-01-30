## 1. DSL Changes

- [x] 1.1 Rename `Import` custom operation to `ImportRaw` in `AgentBuilder`
- [x] 1.2 Add new `Import` custom operation that calls `AST.importRef` (same as `ImportRaw`)

## 2. MarkdownWriter Changes

- [x] 2.1 Add `IncludeCodeBlock` case to `ImportInclusion` discriminated union
- [x] 2.2 Add helper function to map `DataFormat` to language tag string (yaml/json/toon/empty)
- [x] 2.3 Implement code-block wrapping in `writeNode` for `Imported` nodes when `IncludeCodeBlock` is set

## 3. Tests

- [x] 3.1 Add test for `importRaw` DSL operation creating correct AST node
- [x] 3.2 Add test for `import` DSL operation creating correct AST node
- [x] 3.3 Add test for `IncludeCodeBlock` with JSON format wrapping content in ```json
- [x] 3.4 Add test for `IncludeCodeBlock` with YAML format wrapping content in ```yaml
- [x] 3.5 Add test for `IncludeCodeBlock` with TOON format wrapping content in ```toon
- [x] 3.6 Add test for `IncludeCodeBlock` with Unknown format using plain ``` fence

## 4. Documentation

- [x] 4.1 Update README example to show `import` and `importRaw` usage
- [x] 4.2 Document breaking change in release notes (import → importRaw migration)
