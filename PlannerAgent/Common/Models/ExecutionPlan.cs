
using Shared.Models;
namespace PlannerAgent.Common.Models;

public class ExecutionPlan
{
    public string UserRequest { get; set; } = "";
    public AgentGraph Graph { get; set; } = new();
    public Dictionary<string, AgentResponse> Responses { get; set; } = [];
}