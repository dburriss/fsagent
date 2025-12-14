## 1. Specifications
- [ ] 1.1 Draft `agent-dsl` capability spec (CE semantics)
- [ ] 1.2 Draft `agent-ast` delta (frontmatter helpers + import inference)
- [ ] 1.3 Validate change via `openspec validate add-top-level-agent-dsl --strict`

## 2. Implementation
- [ ] 2.1 Implement `agent { ... }` CE that yields Agent AST
- [ ] 2.2 Implement DSL `meta { ... }` builder
  - [ ] `kv: string -> string -> unit`
  - [ ] `kvList: string -> string list -> unit`
  - [ ] `kvObj: string -> Map<string, obj> -> unit`
  - [ ] `kvListObj: string -> obj list -> unit`
- [ ] 2.3 Implement DSL section ops: `role`, `objective`, `instructions`, `context`, `output`, `examples` with `example`, plus generic `section name content`
- [ ] 2.4 Add AST helpers: `fmStr`, `fmNum`, `fmBool`, `fmList`, `fmMap`, `importRef` (format inferred from filename)
- [ ] 2.5 Ensure deterministic ordering (frontmatter first; sections as declared)
- [ ] 2.6 Keep DSL pure; push I/O to writers/import pipeline

## 3. Tests & Validation
- [ ] 3.1 Unit tests: AST construction determinism and shapes (update `AstTests.fs`)
- [ ] 3.2 DSL tests: CE builds expected AST for common patterns from `knowledge/`
- [ ] 3.3 Import inference tests: `.json/.yaml/.yml/.toon` resolve formats correctly
- [ ] 3.4 Typed metadata (kvList/kvObj/kvListObj) including `mcp-servers` nesting
- [ ] 3.5 Writer tests: frontmatter expansion (OpenCode vs Copilot vs Claude) remains correct
- [ ] 3.6 Golden-file tests: rendered Markdown for simple agent definitions
- [ ] 3.7 `dotnet build` and `dotnet test` pass

## 4. Docs & Changelog
- [ ] 4.1 Update `ARCHITECTURE.md` with DSL layer overview
- [ ] 4.2 Update `README.md` with usage samples
- [ ] 4.3 Update `CHANGELOG.md` with feature addition
