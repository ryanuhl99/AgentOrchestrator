using PlannerAgent.Common.Enums;
using PlannerAgent.Common.Models;

namespace PlannerAgent.Services.Clients.Agents;

public interface IAgent
{
    Task<AgentResponse> ExecuteAsync(AgentTask task);
}