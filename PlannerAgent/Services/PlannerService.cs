
using PlannerAgent.Common.Models;
using PlannerAgent.Common.Utils;
using PlannerAgent.Services.Clients.Agents;

namespace PlannerAgent.Services;

public class PlannerService(AgentResolver resolver)
{
    private readonly AgentResolver _resolver = resolver;
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

        while (taskQueue.Count > 0 && inFlight.Count > 0)
        {
            while (taskQueue.Count > 0)
            {
                var task = taskQueue.Dequeue();
                IAgent client = _resolver.Resolve(task.AgentType);
                var ranTask = client.ExecuteAsync(task);
                inFlight[ranTask] = task;
            }

        }
    }
}