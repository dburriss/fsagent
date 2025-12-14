## 1. Implementation
- [x] 1.1 Create `markdown-writer` module and options type
- [x] 1.2 Implement writer: Agent AST → output string (returns string)
- [x] 1.3 Support output formats: `opencode` (default) and `copilot`
- [x] 1.4 Implement imported data inclusion: `none` and `raw` (no auto codeblocks)
- [x] 1.5 Support heading renames and formatter (ATX headings only for md)
- [x] 1.6 Implement optional generated footer function with context
- [x] 1.7 Add output type selection: `md | json | yaml` (md default)
- [x] 1.8 Add custom writer option to override built-in behavior (e.g., Claude writer)
- [x] 1.9 Add unit tests (A/B/C categories) for default and configured cases
- [x] 1.10 Validate deterministic output order
- [x] 1.11 Document usage in README and update CHANGELOG
- [x] 1.12 Add examples referencing `knowledge/import-data.md` and `knowledge/import-data.rules.json`

## 2. Validation
- [x] 2.1 Run `dotnet build` and `dotnet test`
- [ ] 2.2 Run `openspec validate add-markdown-writer --strict`

## 3. Notes
- Keep writer pure (returns string); external code handles file I/O.
- Emit Copilot minimum frontmatter (`name`, `description`) and OpenCode (`description`) when configured.
