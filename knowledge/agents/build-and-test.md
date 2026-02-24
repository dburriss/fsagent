```bash
dotnet build                    # Build project
dotnet build -c Release         # Build in release mode
dotnet test                     # Run unit tests
dotnet test --filter "DisplayName~inferFormat"  # Run specific test by name
fsx scripts/build.fsx           # Alternative build script
```

## Testing Guidelines
- Default to TDD.
- Prefer in-memory tests.
- Run build and tests before and after modifications.
- Use Assert.Fail("message") instead of Assert.True(false, "message") for test failures.

### ABC Test Categories
**A – Acceptance Tests**  
Validate agent generation and writer output. Test DSL→AST→Writer pipeline end-to-end with fast, in-memory execution.

**B – Building Tests**  
Temporary scaffolding during TDD, debugging, or exploration. These tests are disposable.

**C – Communication Tests**  
Validate external boundaries: YAML/JSON parsing, TOON serialization, file I/O for imports, writer frontmatter generation. Keep separate to avoid polluting acceptance tests.

Label tests by external dependencies (e.g., `yaml`, `json`, `filesystem`, `serialization`) to make execution constraints explicit.

Tests use xUnit with naming convention: `[Category]: Description`.