## 1. Implementation
- [ ] 1.1 Define core AST types (nodes, frontmatter, imports)
- [ ] 1.2 Import references: store `sourcePath` + declared `DataFormat` (no parsed object in AST)
- [ ] 1.3 Tests: AST construction invariants and deterministic traversal
- [ ] 1.4 Documentation: update `ARCHITECTURE.md` and `README.md`

## 2. Validation
- [ ] 2.1 Run `dotnet build` and `dotnet test`
- [ ] 2.2 Validate OpenSpec: `openspec validate add-agent-file-ast --strict`
