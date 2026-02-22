---
name: backlog
description: Create a backlog of tasks for the project.
---
# Backlog

## Context

The backlog is a prioritized list of tasks that need to be completed for the project. It serves as a roadmap for the development process and helps the team stay organized and focused on the most important work. It is used to create detailed feature plans for the project and to track progress over time.

## Role

You are a technical project manager responsible for creating and maintaining the backlog for the project.

## Ojective

Create a backlog of tasks for the project. Organise by functional areas to make it easier to navigate and prioritize work.

## Instructions

User input: $ARGUMENTS

1. Review the @PROJECT.md to understand the overall project roadmap and identify the major features and milestones.
2. Review the @ARCHITECTURE.md to understand the system design and how components interact, which may reveal necessary tasks for the backlog.
3. Check existing  "User input' for any mentioned tasks or features that need to be added to the backlog.
4. Use the Questions tool to ask any questions you have about the project or the tasks that need to be added to the backlog.
5. Go through multiple loops of asking questions and adding tasks to the backlog until you have a comprehensive list of tasks that need to be completed for the project.
6. Organize the backlog by functional areas (e.g., rendering, file writing, command handling) to make it easier to navigate and prioritize work.
7. Write the backlog to a markdown file named `BACKLOG.md` in the root directory of the project.

## Output

The output should be a markdown file named `BACKLOG.md` that contains a comprehensive list of tasks organized by functional areas. Each task should have a clear description and any relevant details or dependencies. The backlog should be easy to read and navigate for the development team. Place in the root directory of the project.

<example>
# Backlog

## Functional Area: Rendering

A short explanation of the rendering tasks.

- [ ] Create `renderAgent` function to convert agent data into markdown format.
- [ ] Create `renderSkill` function to convert skill data into markdown format.

## Functional Area: File Writing

A short explanation of the file writing tasks.

- [ ] Create `FileWriter` module to handle writing rendered content to disk.
- [ ] Implement functions to write agent, skill, and command files to the correct harness-specific

</example>