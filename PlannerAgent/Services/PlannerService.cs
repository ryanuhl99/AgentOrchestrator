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

        foreach (var node in graph.Tasks)
        {
            if (node.Dependents.Count == 0)
            {
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
                    var agent = _resolver.Resolve(task.AgentType);
                    var runTask = agent.ExecuteAsync(task);
                    inFlight[runTask] = task;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    task.TaskState = TaskStateEnum.Failed;
                    // need to log workflow errors here, not agent errors
                    _logger.LogAgentError(
                        ex,
                        task.AgentType.ToString(),
                        task.Id,
                        0,
                        0,
                        sw.ElapsedMilliseconds,
                        false
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
                // need to log workflow errors here, not agent errors
                _logger.LogAgentError(
                    ex,
                    finishedAgentTask.AgentType.ToString(),
                    finishedAgentTask.Id,
                    0,
                    0,
                    0,
                    false
                );

                continue;
            }

            finishedAgentTask.TaskState = TaskStateEnum.Completed;

            completed[finishedAgentTask.Id] = response;
            var finishedId = finishedAgentTask.Id;

            // check dependents
            foreach (var dependent in graph.Tasks.Where(t => t.Dependents.Contains(finishedId)))
            {
                if (dependent.TaskState == TaskStateEnum.Pending &&
                    dependent.Dependents.All(dep => completed.ContainsKey(dep) &&
                        completed[dep].IsSuccess))
                {
                    taskQueue.Enqueue(dependent);
                }
            }
        }

        return new ExecutionPlan
        {
            UserRequest = responseObj.UserPrompt,
            Graph = graph,
            Responses = completed
        };
    }
}