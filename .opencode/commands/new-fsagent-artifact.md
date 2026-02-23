---
description: Create a new agent, command, or skill with the fsagent DSL. Usage: /new-fsagent-artifact <type> <name>
---

# Task

Create a new fsagent artifact.

Arguments: $ARGUMENTS

Steps:
1. Parse the arguments: the first word is the artifact type (agent | command | skill),
   the remainder is the desired name.
2. Invoke the `fsagent-author` sub-agent with that context.
3. The sub-agent will load the appropriate skill and produce a runnable .fsx script.
4. Run the script and confirm the written path.
