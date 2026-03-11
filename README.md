# Written Answers

## Q1. Walk us through one scheduling decision your code makes. Pick a specific moment where Task B waits for Task A. Explain exactly how your code knows to wait, what data structure drives that decision, and what happens when Task A fails.

A1. The scheduler makes dependency decisions inside the PlannerService while executing the task graph produced by decomposition. Three primary data structures drive scheduling decisions: 

    - Queue<AgentTask> taskQueue
    - Dictionary<Task<AgentResponse>, AgentTask> inFlight
    - Dictionary<string, AgentResponse> completed

taskQueue contains tasks ready to execute, inFlight tracks currently running agent calls, and completed stores results from finished tasks indexed by task ID.

At the start of execution, the scheduler identifies root tasks (tasks with no dependencies) and enqueues them into taskQueue. These tasks are dispatched immediately and run concurrently. Each running task is tracked in the inFlight map.

The scheduler then waits for the first task to complete using:

```csharp
await Task.WhenAny(inFlight.Keys)
```

(notice how we don't use Task.WhenAll - the reason being, WhenAll would block until ALL root tasks were completed, which is less flexible than the behavior we have here, which is, block until any one task is complete, then proceed down the dependency chain asynchronously)

When a task finishes, its result is recorded in the completed map, at which point, the scheduler evaluates whether the completed task has any dependent tasks that are eligible to be enqueued for processing.

A dependent task (Task B) is only scheduled if all of its dependencies have completed successfully, which is checked using:

```csharp
if (dependent.TaskState == TaskStateEnum.Pending &&
    dependent.Dependencies.All(dep => completed.ContainsKey(dep) &&
    completed[dep].IsSuccess))
```

This condition ensures that Task B will not be scheduled until Task A's result is available.

If Task A fails, the scheduler records the failure in the completed map, but the IsSuccess condition prevents Task B from being scheduled. This allows the orchestration run to continue without crashing while ensuring that tasks that rely on failed outputs are not executed with invalid input.

This mechanism enables a mix of parallel execution for independent tasks and sequential execution for dependent tasks, allowing the scheduler to process the dependency graph efficiently while respecting task ordering constraints.


## Q2. What is the worst-case execution time of your scheduler, and why? Give a concrete answer in terms of the agent latencies provided. Show your reasoning.

A2. The worst-case execution time that could arise from this program is the summation of worst-case time spans from a full dependency chain. In this case, this would be a three-task dependency chain (the longest a chain could be here is research => code => review) all taking as long as they could, which, based on the prompt, is 15 + 6 + 2 seconds = 23 in total. The reason why 23 seconds is the longest possible execution time, even in cases when a prompt contains numerous simultaneous task requests, is because my code enables parallel execution of root tasks (no dependencies) and leverages async / interleaving when multiple root tasks have children that are needing completion around the same time. The scheduler dispatches those tasks in parallel and waits only on the completion of the longest dependency chain. As a result, additional parallel tasks do not increase the worst-case runtime unless they extend the critical path / dependency chain.


## Q3. Write out your synthesis prompt and explain each section's purpose. What did you include? What did you deliberately leave out? Why?

A3. My entire synthesis prompt is as follows (i will break down each section below):

        var prompt = $$"""
        You are a task planning AI responsible for decomposing developer requests
        into executable tasks for specialized agents.

        AVAILABLE AGENTS

        ResearchAgent
        - Retrieves and summarizes documentation, GitHub issues, StackOverflow threads, internal wiki pages, and other similar resources.

        CodeAgent
        - Generates, refactors, or explains code given a specification.

        ReviewAgent
        - Reviews code or text output for correctness, security issues, and style violations.

        Note: ResearchAgent, CodeAgent, and ReviewAgent are required naming conventions for agent_type field specified below.

        YOUR JOB

        Break the user request into a sequence of tasks assigned to these agents.

        DEPENDENCY RULES

        1. If a task requires information produced by another task,
        it must list that task in its "dependencies" field.

        2. Tasks with no dependencies can run in parallel.

        3. Code review tasks must depend on the code generation task.

        4. If the request includes research followed by coding,
        the coding task should depend on the research task.

        5. Dependencies must form a Directed Acyclic Graph (DAG).
        Tasks cannot depend on themselves and circular dependencies are not allowed.

        Important: dependencies should be evaluated and determined carefully. It is possible for a prompt to contain a request for code research on one particular topic, and a request for code evaluation that is unrelated to said research. 

        OUTPUT FORMAT

        Return JSON only.
        Do not include markdown formatting or code blocks.

        {
        "tasks": [
            {
            "id": "task1",
            "agent_type": "ResearchAgent",
            "prompt": "...",
            "dependencies": []
            }
        ]
        }

        USER REQUEST

        {{request.PromptRequest}}
        """;

    
The planner agent uses an LLM prompt to transform a developer request into a structured execution graph. The prompt is written inline to satisfy the assignment constraint and is designed to guide the model toward producing a valid task graph with clear agent assignments and dependency relationships.

The prompt is structured into several sections, each serving a specific purpose. They are as follows:

### 1. System Role Definition

    'You are a task planning AI responsible for decomposing developer requests
    into executable tasks for specialized agents.'

This section establishes the model’s role and narrows its objective to task decomposition rather than answering the request itself. Without this instruction, the LLM might attempt to directly solve the input request rather than produce an orchestration plan.

### 2. Available Agent Definitions

    'AVAILABLE AGENTS

    ResearchAgent
    - Retrieves and summarizes documentation, GitHub issues, StackOverflow threads, internal wiki pages, and other similar resources.

    CodeAgent
    - Generates, refactors, or explains code given a specification.

    ReviewAgent
    - Reviews code or text output for correctness, security issues, and style violations.'

This section defines the capabilities and responsibilities of each specialized agent.
The purpose is twofold:

    - Provide semantic context for how tasks should be assigned.

    - Prevent the LLM from inventing new agent types or assigning inappropriate work to the wrong agent.

Explicit capability descriptions help the model map requests into the correct agent categories.

### 3. Task Decomposition Instruction

    'YOUR JOB
    Break the user request into a sequence of tasks assigned to these agents.'

This reinforces that the output should represent a workflow, not a direct answer.

The LLM is explicitly instructed to produce multiple tasks, which encourages decomposition of complex requests like: “Research a library, write an integration example, and review it.”, into discrete steps.

### 4. Dependency Rules

These rules constrain the model to produce valid orchestration plans:

Rule 1 — Explicit dependency representation

    'If a task requires information produced by another task,
    it must list that task in its "dependencies" field.'

This ensures that task ordering is expressed through the dependency graph rather than implied through position.
The scheduler relies on this field to determine when tasks become eligible for execution.

Rule 2 — Parallelizable tasks

    'Tasks with no dependencies can run in parallel.'

This emphasizes to the model to avoid unnecessary dependencies so the scheduler can maximize parallelization.

Rule 3 — Review tasks depend on code generation

    'Code review tasks must depend on the code generation task.'

This prevents logically invalid plans like attempting to review code before it has been generated.

Rule 4 — Research precedes implementation

    'If the request includes research followed by coding,
    the coding task should depend on the research task.

This structures a standard developer workflow: gather information first, then implement code; also, reinforces logically coherent structure.

Rule 5 — DAG constraint

    'Dependencies must form a Directed Acyclic Graph (DAG).
    Tasks cannot depend on themselves and circular dependencies are not allowed.

This rule prevents the LLM from generating cyclic/circular dependencies that would make the scheduler impossible to execute.

### Additional clarification

    'It is possible for a prompt to contain a request for code research on one topic and code evaluation on an unrelated topic.'

This instruction gives extra emphasis to help the LLM avoid over-connecting tasks. Some requests may contain multiple independent subtasks that should not depend on each other.

### 5. Output Format Specification

    'Return JSON only.
    Do not include markdown formatting or code blocks.'

It is my understanding that LLMs can often wrap JSON responses in Markdown. This instruction helps ensure the response can be parsed directly by the application without additional preprocessing.

### JSON Schema

```json
{
  "tasks": [
    {
      "id": "task1",
      "agent_type": "ResearchAgent",
      "prompt": "...",
      "dependents": []
    }
  ]
}
```

I provide an explicit schema here to help the LLM produce consistent, parsable output able to deserialize nicely into a data transfer object. The scheduler expects:

    - a unique id

    - a valid agent_type

    - a prompt for the agent

    - a dependency list

This schema aligns directly with the AgentTask model.

### 6. User Request Injection

    'USER REQUEST
    {{request.PromptRequest}}'

The user's request is injected at the end of the prompt so the model can perform decomposition.

Placing the request after the rules ensures the model reads the structural constraints before processing the request.


### Design Considerations

What was included:

    - explicit agent capabilities

    - dependency rules / structure

    - DAG constraints

    - strict JSON output schema

These design considerations are meant to reduce ambiguity and improve the reliability of structured outputs.

What was deliberately omitted:

    - detailed implementation instructions for each agent

    - strict limits on the number of tasks

    - rigid task ordering

These were omitted to allow the LLM to flexibly decompose requests with varying complexity.


# NOTE:
### In this implementation the scheduler enforces dependency ordering before dispatching dependent tasks. In a production system, outputs from dependency tasks would be injected into the dependent task’s prompt to provide additional context for downstream agents.