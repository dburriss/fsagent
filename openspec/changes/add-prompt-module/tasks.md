## 1. Dependencies and Project Setup

- [ ] 1.1 Add Fue NuGet package (version 2.2.0) to src/FsAgent/FsAgent.fsproj
- [ ] 1.2 Update FsAgent.fsproj compilation order to include AST.fs, Prompt.fs, Agent.fs, Writers.fs, Library.fs

## 2. Create AST.fs (Foundation Layer)

- [ ] 2.1 Create src/FsAgent/AST.fs file with FsAgent.AST namespace
- [ ] 2.2 Move DataFormat enum from Library.fs to AST.fs
- [ ] 2.3 Move Node discriminated union from Library.fs to AST.fs
- [ ] 2.4 Add Template case to Node DU (contains text: string)
- [ ] 2.5 Add TemplateFile case to Node DU (contains path: string)
- [ ] 2.6 Move AST module from Library.fs to AST.fs
- [ ] 2.7 Remove prompt-specific constructors (role, objective, instructions, context, output, example, examples) from AST module
- [ ] 2.8 Keep inferFormat, importRef, importRawRef, and frontmatter helpers (fmStr, fmNum, fmBool, fmList, fmMap) in AST module

## 3. Create Prompt.fs (Prompt Domain)

- [ ] 3.1 Create src/FsAgent/Prompt.fs file with FsAgent.Prompts namespace
- [ ] 3.2 Define Prompt type with Frontmatter: Map<string, obj> and Sections: Node list
- [ ] 3.3 Create Prompt module with empty constructor
- [ ] 3.4 Move prompt constructors from AST to Prompt module (role, objective, instructions, context, output, example, examples)
- [ ] 3.5 Create PromptBuilder module with AutoOpen attribute
- [ ] 3.6 Implement PromptBuilder computation expression (Yield, Run)
- [ ] 3.7 Add metadata operations to PromptBuilder (meta, name, description, author, version, license)
- [ ] 3.8 Add content operations to PromptBuilder (role, objective, instructions, context, output, examples, section)
- [ ] 3.9 Add import operations to PromptBuilder (import, importRaw)
- [ ] 3.10 Add template operations to PromptBuilder (template, templateFile)
- [ ] 3.11 Create prompt builder instance

## 4. Create Agent.fs (Agent Domain)

- [ ] 4.1 Create src/FsAgent/Agent.fs file with FsAgent.Agents namespace
- [ ] 4.2 Define Agent type with Frontmatter: Map<string, obj> and Sections: Node list
- [ ] 4.3 Create Agent module with empty constructor
- [ ] 4.4 Create AgentBuilder module with AutoOpen attribute
- [ ] 4.5 Move MetaBuilder from Library.fs to AgentBuilder module
- [ ] 4.6 Create meta builder instance
- [ ] 4.7 Implement AgentBuilder computation expression (Yield, Run)
- [ ] 4.8 Add meta operation to AgentBuilder
- [ ] 4.9 Add metadata operations to AgentBuilder (name, description, author, version, license, model, temperature, maxTokens, tools)
- [ ] 4.10 Add prompt operation to AgentBuilder (accepts Prompt, merges sections)
- [ ] 4.11 Add section and import operations to AgentBuilder (section, import, importRaw)
- [ ] 4.12 Remove role, objective, instructions, context, output, examples operations from AgentBuilder
- [ ] 4.13 Create agent builder instance

## 5. Create Writers.fs (Serialization Layer)

- [ ] 5.1 Create src/FsAgent/Writers.fs file with FsAgent.Writers namespace
- [ ] 5.2 Create Template module with TemplateVariables type alias (Map<string, obj>)
- [ ] 5.3 Implement Template.renderInline function (text -> variables -> string) using Fue
- [ ] 5.4 Implement Template.renderFile function (path -> variables -> string) using Fue
- [ ] 5.5 Add error handling to renderInline (return "[Template error: ...]" on exception)
- [ ] 5.6 Add error handling to renderFile (return "[Template file not found: ...]" or "[Template error: ...]")
- [ ] 5.7 Move MarkdownWriter module from Library.fs to Writers.fs
- [ ] 5.8 Keep existing types (AgentFormat, OutputType, WriterContext, Options)
- [ ] 5.9 Add TemplateVariables field to Options type (mutable, default: Map.empty)
- [ ] 5.10 Update defaultOptions function to include TemplateVariables = Map.empty
- [ ] 5.11 Move existing private helper functions to Writers.fs (formatFrontmatter, loadImportContent, nodeToObj, etc.)
- [ ] 5.12 Extend writeMd function to handle Template nodes (call Template.renderInline with opts.TemplateVariables)
- [ ] 5.13 Extend writeMd function to handle TemplateFile nodes (call Template.renderFile with opts.TemplateVariables)
- [ ] 5.14 Extend nodeToObj function for JSON/YAML serialization to handle Template and TemplateFile nodes
- [ ] 5.15 Create writePrompt function (Prompt -> (Options -> unit) -> string)
- [ ] 5.16 In writePrompt, convert Prompt to Agent-like structure with Frontmatter = Map.empty
- [ ] 5.17 In writePrompt, delegate to writeMd/writeJson/writeYaml based on OutputType
- [ ] 5.18 Keep writeMarkdown function as alias to writeAgent for backward compatibility

## 6. Update Library.fs (Backward Compatibility Layer)

- [ ] 6.1 Remove types and modules that moved to other files
- [ ] 6.2 Add type aliases in FsAgent namespace (DataFormat, Node, Agent, Prompt)
- [ ] 6.3 Create DSL module with re-exports (meta, agent, prompt builders)
- [ ] 6.4 Create AST module re-export (pointing to AST.AST)
- [ ] 6.5 Create MarkdownWriter module re-export (pointing to Writers.MarkdownWriter)

## 7. Create Test Files

- [ ] 7.1 Create tests/FsAgent.Tests/PromptTests.fs
- [ ] 7.2 Write acceptance tests for prompt DSL → Prompt → writePrompt pipeline
- [ ] 7.3 Write building tests for PromptBuilder operations
- [ ] 7.4 Write communication tests for prompt format validation
- [ ] 7.5 Create tests/FsAgent.Tests/TemplateTests.fs
- [ ] 7.6 Write tests for Template.renderInline with variable substitution
- [ ] 7.7 Write tests for Template.renderFile with file loading
- [ ] 7.8 Write tests for template error handling (missing files, syntax errors)
- [ ] 7.9 Create tests/FsAgent.Tests/AgentPromptIntegrationTests.fs
- [ ] 7.10 Write tests for agent referencing prompt via prompt operation
- [ ] 7.11 Write tests for multiple prompts referenced in single agent
- [ ] 7.12 Write tests for prompt sections merged into agent sections

## 8. Update Existing Test Files

- [ ] 8.1 Update tests/FsAgent.Tests/AstTests.fs imports to open FsAgent.AST
- [ ] 8.2 Remove tests for role, objective, instructions, context, output, example, examples from AstTests.fs
- [ ] 8.3 Add tests for Template and TemplateFile node cases in AstTests.fs
- [ ] 8.4 Update tests/FsAgent.Tests/DslTests.fs imports to open FsAgent.Agents
- [ ] 8.5 Remove tests for agent builder role/objective/instructions operations from DslTests.fs
- [ ] 8.6 Add tests for agent builder prompt operation in DslTests.fs
- [ ] 8.7 Add tests for agent builder model/temperature/maxTokens/tools operations in DslTests.fs
- [ ] 8.8 Update tests/FsAgent.Tests/MarkdownWriterTests.fs imports to open FsAgent.Writers
- [ ] 8.9 Add backward compatibility tests for writeMarkdown in MarkdownWriterTests.fs
- [ ] 8.10 Add tests for Template/TemplateFile node rendering in MarkdownWriterTests.fs
- [ ] 8.11 Add tests for TemplateVariables in Options in MarkdownWriterTests.fs

## 9. Update Test Project Configuration

- [ ] 9.1 Update tests/FsAgent.Tests/FsAgent.Tests.fsproj compilation order
- [ ] 9.2 Add PromptTests.fs to compilation order
- [ ] 9.3 Add TemplateTests.fs to compilation order
- [ ] 9.4 Add AgentPromptIntegrationTests.fs to compilation order

## 10. Verification and Documentation

- [ ] 10.1 Run dotnet build and verify no compilation errors
- [ ] 10.2 Run dotnet test and verify all tests pass
- [ ] 10.3 Test backward compatibility by running existing agent builder code
- [ ] 10.4 Create example usage of new prompt builder API
- [ ] 10.5 Create example usage of agent referencing prompts
- [ ] 10.6 Create example usage of template rendering with variables
- [ ] 10.7 Verify prompts don't output frontmatter blocks in markdown
- [ ] 10.8 Verify template rendering happens at write time with configured variables
