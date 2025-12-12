## 1. Implementation
- [x] 1.1 Define core AST types (nodes, frontmatter, imports)
- [x] 1.2 Import references: store `sourcePath` + declared `DataFormat` (no parsed object in AST)
- [x] 1.3 Tests: AST construction invariants and deterministic traversal
- [x] 1.4 Documentation: update `ARCHITECTURE.md` and `README.md`
- [x] 1.5 Implement AST constructors: `role`, `objective`, `instructions`, `context`, `output`, `example`, `examples` (pure functions)

## 2. Validation
- [x] 2.1 Run `dotnet build` and `dotnet test`
- [x] 2.2 Validate OpenSpec: `openspec validate add-agent-file-ast --strict`
