namespace FsAgent

// Type aliases for backward compatibility
type DataFormat = AST.DataFormat
type Node = AST.Node
type Agent = Agents.Agent
type Prompt = Prompts.Prompt

// Re-export DSL builders
module DSL =
    let meta = Agents.AgentBuilder.meta
    let agent = Agents.AgentBuilder.agent
    let prompt = Prompts.PromptBuilder.prompt

// Re-export AST module
module AST = FsAgent.AST.AST

// Re-export MarkdownWriter module
module MarkdownWriter = FsAgent.Writers.MarkdownWriter
