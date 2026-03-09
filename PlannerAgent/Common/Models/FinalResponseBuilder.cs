
using PlannerAgent.Common.Enums;
using System.Text;

namespace PlannerAgent.Common.Models;

public static class FinalResponseBuilder
{
    public static RunPromptResponse Build(ExecutionPlan exePlan)
    {
        var sb = new StringBuilder();
        var tasks = exePlan.Graph.Tasks;
        var responses = exePlan.Responses;

        var rootTasks = tasks.Where(t => t.Dependents.Count == 0);

        foreach (var root in rootTasks)
        {
            TraverseTask(root, tasks, responses, sb);
        }

        return new RunPromptResponse
        {
            PromptResponse = sb.ToString()
        };
    }

    private static void TraverseTask(
    AgentTask task,
    List<AgentTask> tasks,
    Dictionary<string, AgentResponse> responses,
    StringBuilder sb)
    {
        responses.TryGetValue(task.Id, out var response);
        response ??= new AgentResponse();

        if (task.TaskState != TaskStateEnum.Completed || !response.IsSuccess)
        {
            var err = string.IsNullOrWhiteSpace(response.Error)
                ? "Incomplete Task"
                : response.Error;

            sb.AppendLine($"{task.AgentType}:");
            sb.AppendLine("Status: Failed");
            sb.AppendLine($"Error: {err}");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine(response.Output);
            sb.AppendLine();
        }

        // find children
        var children = tasks.Where(t => t.Dependents.Contains(task.Id));

        foreach (var child in children)
        {
            TraverseTask(child, tasks, responses, sb);
        }
    }
}