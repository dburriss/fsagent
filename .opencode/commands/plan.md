---
name: plan
description: Plan next feature based on BACKLOG and design docs
---
# Plan creation

## Context

The `plans/` directory contains markdown files outlining detailed plans for implementing upcoming features. 

## Role

You are a software engineer on the team with a focus on agile project management and technical documentation.

## Objective

Use the @BACKLOG.md and design docs to create a plan for the next feature to implement. $ARGUMENTS

## Instructions

1. Review @ARCHITECTURE.md for the system design and how components interact.
2. Review @BACKLOG.md for the current list of features and tasks.
3. Determine the next feature by identifying the next most logical step in the roadmap that has not yet been implemented. This could be a single item or a small set of related items that can be completed together.
4. Create a detailed plan for implementing the feature, including:
    - A Status: Draft indicator at the top of the file.
    - A clear description of the feature and its purpose.
    - Scope of the work involved, including any specific tasks or steps needed to implement the feature.
    - Any dependencies or prerequisites that must be addressed before starting.
    - Impact on existing code and any necessary refactoring.
    - Acceptance criteria for when the feature can be considered complete.
    - Testing strategy to ensure the feature works as intended and does not break existing functionality.
    - Risks or challenges that may arise during implementation and how to mitigate them.
    - Open questions that need to be resolved before or during implementation.
6. Save the plan in a new file in the `plans/` directory with a descriptive name (e.g., `task-list-command.md`).
7. Use the Question tool to ask any clarifying questions needed to fill in gaps in the plan or resolve open questions.

## Constraints

- The plan should be detailed enough to guide the implementation process without needing to refer back to the design docs.
- The plan should not be verbose; focus on clarity and actionable steps. 2-page maximum is ideal.
- Do not implement the feature or write any code; this is strictly for planning.

## Output

The output should be a markdown file in the `plans/` directory with a detailed plan for implementing the next feature, following the structure outlined in step 4. The file should be well-organized and clearly written to guide the implementation process.
