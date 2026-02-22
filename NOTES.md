# Notes

- there was a recommendation to use JsonElement or something like that in the Metadata map so that it is not lossy
- skills can go in .agents/ folder for some agent harnesses
- command can take a model
- what about command parameters? are these harness dependent?
- i want to be ablet to serialize into prompt, agent, skill, and command
- maybe write some agents using the lib to work with improving code, testing
- write slash commandsd to streamline workflow with planning
- write skills for using the lib to create agents, commands, and skills
- config file for global copilot location
- there was an abstraction lib for file io that I could use to make tests easier and avoid direct file system access in the library code
- need an `AgentFileWriter` that composes the render and write.