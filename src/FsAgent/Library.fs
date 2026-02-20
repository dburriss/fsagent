namespace FsAgent

// Type aliases for backward compatibility
type DataFormat = AST.DataFormat
type Tool = Tools.Tool
type Node = AST.Node
type Agent = Agents.Agent
type Prompt = Prompts.Prompt
type SlashCommand = Commands.SlashCommand
type Skill = Skills.Skill

// Re-export DSL builders
module DSL =
    let meta = Agents.AgentBuilder.meta
    let agent = Agents.AgentBuilder.agent
    let prompt = Prompts.PromptBuilder.prompt
    let command = Commands.CommandBuilder.command
    let skill = Skills.SkillBuilder.skill

// Re-export modules
module AST = FsAgent.AST.AST
module AgentWriter = FsAgent.Writers.AgentWriter
