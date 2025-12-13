## 1. Implementation
- [ ] 1.1 Create `markdown-writer` module and options type
- [ ] 1.2 Implement writer: Agent AST → output string (returns string)
- [ ] 1.3 Support output formats: `opencode` (default) and `copilot`
- [ ] 1.4 Implement imported data inclusion: `none` and `raw` (no auto codeblocks)
- [ ] 1.5 Support heading renames and formatter (ATX headings only for md)
- [ ] 1.6 Implement optional generated footer function with context
- [ ] 1.7 Add output type selection: `md | json | yaml` (md default)
- [ ] 1.8 Add custom writer option to override built-in behavior (e.g., Claude writer)
- [ ] 1.9 Add unit tests (A/B/C categories) for default and configured cases
- [ ] 1.10 Validate deterministic output order
- [ ] 1.11 Document usage in README and update CHANGELOG
- [ ] 1.12 Add examples referencing `knowledge/import-data.md` and `knowledge/import-data.rules.json`

## 2. Validation
- [ ] 2.1 Run `dotnet build` and `dotnet test`
- [ ] 2.2 Run `openspec validate add-markdown-writer --strict`

## 3. Notes
- Keep writer pure (returns string); external code handles file I/O.
- Emit Copilot minimum frontmatter (`name`, `description`) and OpenCode (`description`) when configured.
