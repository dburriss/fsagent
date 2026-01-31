## 1. AST Changes

- [x] 1.1 Add `wrapInCodeBlock: bool` to `Imported` node type
- [x] 1.2 Add `AST.importRawRef` function that creates `Imported` with `wrapInCodeBlock=false`
- [x] 1.3 Update `AST.importRef` to set `wrapInCodeBlock=true`

## 2. DSL Changes

- [x] 2.1 Update `import` custom operation to use `AST.importRef` (wrapInCodeBlock=true)
- [x] 2.2 Add `importRaw` custom operation to use `AST.importRawRef` (wrapInCodeBlock=false)

## 3. MarkdownWriter Changes

- [x] 3.1 Remove `ImportInclusion` type entirely (imports always resolved)
- [x] 3.2 Add `DisableCodeBlockWrapping: bool` option (default false)
- [x] 3.3 Update writer to respect `wrapInCodeBlock` flag from AST node
- [x] 3.4 Update writer to honor `DisableCodeBlockWrapping` option

## 4. Tests

- [x] 4.1 Add test for `AST.importRef` creating node with `wrapInCodeBlock=true`
- [x] 4.2 Add test for `AST.importRawRef` creating node with `wrapInCodeBlock=false`
- [x] 4.3 Add test for `import` DSL operation with `wrapInCodeBlock=true`
- [x] 4.4 Add test for `importRaw` DSL operation with `wrapInCodeBlock=false`
- [x] 4.5 Add test for import wrapping JSON in ```json fence
- [x] 4.6 Add test for import wrapping YAML in ```yaml fence
- [x] 4.7 Add test for import wrapping TOON in ```toon fence
- [x] 4.8 Add test for import using plain ``` for Unknown format
- [x] 4.9 Add test for `importRaw` embedding without code fences
- [x] 4.10 Add test for `DisableCodeBlockWrapping` forcing raw output
- [x] 4.11 Update default options test for new defaults

## 5. Documentation

- [x] 5.1 Update README with new import/importRaw usage
- [x] 5.2 Update CHANGELOG with breaking changes
- [x] 5.3 Update example script (toon.fsx) to use new API
