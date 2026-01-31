# FsAgent API Examples

This document provides examples of what the FsAgent API can could look like.
It aims to have a declarative API that allows defining common agentic workflow artifacts such as prompts, agents, slash commands, and skills.

## Structure

The library constists of the following modules:

- Prompts: reusable prompt templates
- Agents: definitions of agents that can use prompts and tools
- Skills: higher-level workflows that can use agents and prompts
- Slash Commands: commands that can be invoked via a slash command interface
- Writers: For writing the supplied components to string

Additionally, there are some lower level modules that can be used.

- AST: Is a lower level module for building up the structure of the artifacts
- Paths: Utilities for working with paths of the artifacts and different agent harnesses

## Prompts

Prompts are reusable prompt templates that can be referenced by agents or skills. FsAgent provides oppinionated structure for defining prompts. Additionally, text can be included, as well as template variables.

```fsharp
let promptExample =
    prompt {
        name "code-review-prompt"
        description "A prompt for reviewing code for bugs and design issues"
        author "Devon Burriss
        version "1.0"
        license "MIT"
        template """This is a template for {{{name}}}."""
        templateFile "prompts/code-review-prompt.txt"
        importRaw "prompts/common-helpers.md"
        import "examples/data.json"
        context "This is some additional context for the code reviewer."
        role "You are a helpful code reviewer."
        objective "Review the code and provide feedback."
        instructions "Focus on bugs and design issues."
        output "Provide your feedback in markdown format."
        examples [
            example 
                "Exmple 1"
                "This is an example of how to use the code review prompt."
            example 
                "Example 2"
                "This is another example of how to use the code review prompt."
        ]
```

Templates are based on [Fue](https://github.com/Dzoukr/Fue).

## Slash Commands

Slash commands are commands that can be invoked via a slash command interface. They can be used to trigger agents or skills.

```fsharp
let slashCommandExample =
    slashCommand {
        name "code-review"
        description "A slash command for reviewing code"
        author "Devon Burriss"
        license "MIT"
        version "1.0"
        prompt promptExample
    }
```

## Skills

Skills are used to teach an agent how to perform certain tasks. They are loaded dynamically by the agent harness as needed.

```fsharp
let skillExample =
    skill {
        name "code-review-skill"
        description "A skill for reviewing code for bugs and design issues"
        author "Devon Burriss"
        license "MIT"
        version "1.0"
        prompt promptExample
    }
```

## Agents

Agents are tailored to perform specific tasks using prompts, and tools. They can be configured with different models and settings.

```fsharp
let agentExample =
    agent {
        name "code-review-agent"
        description "An agent for reviewing code for bugs and design issues"
        author "Devon Burriss"
        license "MIT"
        version "1.0"
        model "gpt-4"
        temperature 0.2
        maxTokens 2000
        tools [
            toolReference "code-search-tool"
            toolReference "code-analysis-tool"
        ]
        prompt promptExample
    }
```

## Writers

Writers are used to serialize the components to string format for saving to files or displaying.

```fsharp
let writerExample = MarkdownWriter()
let promptMarkdown = writerExample.WritePrompt(promptExample)
let agentMarkdown = writerExample.WriteAgent(agentExample)
let skillMarkdown = writerExample.WriteSkill(skillExample)
let slashCommandMarkdown = writerExample.WriteSlashCommand(slashCommandExample)
```

## Readers

Readers allow you to read in an existing artifact from a string or file and parse it into the corresponding FsAgent structure.

This could be useful for loading existing prompts, agents, skills, or slash commands. You could then convert them to use with another agent harness.

```fsharp
let readerExample = MarkdownReader()
let openCodePrompt = readerExample.ReadPrompt("path/to/prompt.md", AgentFormats.OpenCode)

let writer = MarkdownWriter()
let copilotPromptMarkdown = writer.WritePrompt(openCodePrompt, AgentFormats.Copilot)
File.WriteAllText(ConfigPaths.CopilotPrompts.Repository(), copilotPromptMarkdown)
```

## Other Utilities

### Paths

The Paths module provides utilities for working with the paths of the artifacts and different agent harnesses.

`ConfigPaths` contains predefined paths for common agent harnesses like Copilot, OpenCode, and others. It contains helper methods for getting the correct paths for prompts, agents, skills, and slash commands. Paths exist for both local repository locations as well as user-specific locations. OS-specific paths are also handled.

### AST

The AST module provides lower level constructs for building up the structure of the artifacts. It can be used to create custom components or manipulate existing ones at a more granular level.

```fsharp
let customPrompt = Node.Section(
    title = "Custom Section",
    content = [ AST.Text("This is a custom section in the prompt.") ]
)
```
