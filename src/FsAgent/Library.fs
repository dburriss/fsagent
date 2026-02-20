namespace FsAgent

// Type aliases for backward compatibility
type DataFormat = AST.DataFormat
type Tool = Tools.Tool
type Node = AST.Node
type Agent = Agents.Agent
type Prompt = Prompts.Prompt
type SlashCommand = Commands.SlashCommand

// Re-export DSL builders
module DSL =
    let meta = Agents.AgentBuilder.meta
    let agent = Agents.AgentBuilder.agent
    let prompt = Prompts.PromptBuilder.prompt
    let command = Commands.CommandBuilder.command

// Re-export modules
module AST = FsAgent.AST.AST
module MarkdownWriter = FsAgent.Writers.MarkdownWriter
