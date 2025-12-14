## 1. Specifications
- [x] 1.1 Draft `agent-dsl` capability spec (CE semantics)
- [x] 1.2 Draft `agent-ast` delta (frontmatter helpers + import inference)
- [x] 1.3 Validate change via `openspec validate add-top-level-agent-dsl --strict`

## 2. Implementation
- [x] 2.1 Implement `agent { ... }` CE that yields Agent AST
- [x] 2.2 Implement DSL `meta { ... }` builder
  - [x] `kv: string -> string -> unit`
  - [x] `kvList: string -> string list -> unit`
  - [x] `kvObj: string -> Map<string, obj> -> unit`
  - [x] `kvListObj: string -> obj list -> unit`
- [x] 2.3 Implement DSL section ops: `role`, `objective`, `instructions`, `context`, `output`, `examples` with `example`, plus generic `section name content`
- [x] 2.4 Add AST helpers: `fmStr`, `fmNum`, `fmBool`, `fmList`, `fmMap`, `importRef` (format inferred from filename)
- [x] 2.5 Ensure deterministic ordering (frontmatter first; sections as declared)
- [x] 2.6 Keep DSL pure; push I/O to writers/import pipeline

## 3. Tests & Validation
- [x] 3.1 Unit tests: AST construction determinism and shapes (update `AstTests.fs`)
- [x] 3.2 DSL tests: CE builds expected AST for common patterns from `knowledge/`
- [x] 3.3 Import inference tests: `.json/.yaml/.yml/.toon` resolve formats correctly
- [x] 3.4 Typed metadata (kvList/kvObj/kvListObj) including `mcp-servers` nesting
- [x] 3.5 Writer tests: frontmatter expansion (OpenCode vs Copilot vs Claude) remains correct
- [x] 3.6 Golden-file tests: rendered Markdown for simple agent definitions
- [x] 3.7 `dotnet build` and `dotnet test` pass

## 4. Docs & Changelog
- [x] 4.1 Update `ARCHITECTURE.md` with DSL layer overview
- [x] 4.2 Update `README.md` with usage samples
- [x] 4.3 Update `CHANGELOG.md` with feature addition
