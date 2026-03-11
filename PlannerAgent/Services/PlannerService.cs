using System.Diagnostics;
using Infra;
using Shared.Enums;
using Shared.Models;
using PlannerAgent.Common.Models;
using PlannerAgent.Common.Utils;
using PlannerAgent.Services.Clients.Agents;

namespace PlannerAgent.Services;

public class PlannerService(AgentResolver resolver, ILogger<PlannerService> logger)
{
    private readonly AgentResolver _resolver = resolver;
    private readonly ILogger<PlannerService> _logger = logger;

    public async Task<ExecutionPlan> ExecuteTasks(LlmResponseDto responseObj)
    {
        var taskQueue = new Queue<AgentTask>();
        var inFlight = new Dictionary<Task<AgentResponse>, AgentTask>();
        var completed = new Dictionary<string, AgentResponse>();

        var graph = responseObj.Graph;

        _logger.LogInformation("Planner starting execution of graph: {@Graph}", graph);

        foreach (var node in graph.Tasks)
        {
            if (node.Dependencies.Count == 0)
            {
                _logger.LogInformation(
                    "Enqueuing root task {TaskId} for agent {AgentType}",
                    node.Id,
                    node.AgentType
                );
                taskQueue.Enqueue(node);
            }
        }


        while (taskQueue.Count > 0 || inFlight.Count > 0)
        {
            while (taskQueue.Count > 0)
            {
                var sw = Stopwatch.StartNew();
                var task = taskQueue.Dequeue();
                task.TaskState = TaskStateEnum.Running;

                try
                {
                    _logger.LogInformation(
                        "Dispatching task {TaskId} to {AgentType}",
                        task.Id,
                        task.AgentType
                    );

                    var agent = _resolver.Resolve(task.AgentType);
                    var runTask = agent.ExecuteAsync(task);

                    inFlight[runTask] = task;
                }
                catch (Exception ex)
                {
                    task.TaskState = TaskStateEnum.Failed;
    
                    _logger.LogError(
                        ex,
                        "Failed to dispatch task {TaskId} for agent {AgentType}",
                        task.Id,
                        task.AgentType
                    );
                }
            }

            // wait for first task to finish
            var finishedTask = await Task.WhenAny(inFlight.Keys);
            var finishedAgentTask = inFlight[finishedTask];
            inFlight.Remove(finishedTask);

            AgentResponse response;

            try
            {
                response = await finishedTask;
            }
            catch (Exception ex)
            {
                finishedAgentTask.TaskState = TaskStateEnum.Failed;
                _logger.LogError(
                    ex,
                    "Task {TaskId} executed by {AgentType} failed",
                    finishedAgentTask.Id,
                    finishedAgentTask.AgentType
                );

                continue;
            }

            finishedAgentTask.TaskState = TaskStateEnum.Completed;

            completed[finishedAgentTask.Id] = response;

            _logger.LogInformation(
                "Task {TaskId} completed by {AgentType} with success={Success}",
                finishedAgentTask.Id,
                finishedAgentTask.AgentType,
                response.IsSuccess
            );

            var finishedId = finishedAgentTask.Id;

            // check dependents
            foreach (var dependent in graph.Tasks.Where(t => t.Dependencies.Contains(finishedId)))
            {
                if (dependent.TaskState == TaskStateEnum.Pending &&
                    dependent.Dependencies.All(dep => completed.ContainsKey(dep) &&
                        completed[dep].IsSuccess))
                {
                    _logger.LogInformation(
                        "Scheduling dependent task {TaskId} for agent {AgentType}",
                        dependent.Id,
                        dependent.AgentType
                    );

                    var context = dependent.Dependencies
                        .Select(dep => $"Output from {dep}:\n{completed[dep].Output}")
                        .ToList();

                    dependent.Prompt += string.Join("\n\n", context);

                    taskQueue.Enqueue(dependent);
                }
                else
                {
                    _logger.LogDebug(
                        "Dependent task {TaskId} not ready yet. Waiting for remaining dependencies.",
                        dependent.Id
                    );
                }
            }
        }

        _logger.LogInformation("Planner execution finished. Completed {CompletedCount} tasks", completed.Count);

        return new ExecutionPlan
        {
            UserRequest = responseObj.UserPrompt,
            Graph = graph,
            Responses = completed
        };
    }
}