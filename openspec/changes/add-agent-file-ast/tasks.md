## 1. Implementation
- [ ] 1.1 Define core AST types (nodes, frontmatter, imports)
- [ ] 1.2 Add DSL builders that produce AST (no writer coupling)
- [ ] 1.3 Implement baseline Markdown writer over AST (non-flavour)
- [ ] 1.4 Implement JSON writer over AST (non-flavour)
- [ ] 1.5 Add flavour wrappers (OpenCode, Copilot, Claude) delegating to base writers
- [ ] 1.6 Import pipeline integration: store parsed object + source format
- [ ] 1.7 Tests: AST construction invariants and golden-file renders
- [ ] 1.8 Documentation: update `ARCHITECTURE.md` and `README.md`

## 2. Validation
- [ ] 2.1 Run `dotnet build` and `dotnet test`
- [ ] 2.2 Verify golden-file outputs for sample agents
- [ ] 2.3 Validate OpenSpec: `openspec validate add-agent-file-ast --strict`
