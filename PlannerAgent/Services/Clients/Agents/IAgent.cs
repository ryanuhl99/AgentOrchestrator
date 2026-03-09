
using Shared.Models;

namespace PlannerAgent.Services.Clients.Agents;

public interface IAgent
{
    Task<AgentResponse> ExecuteAsync(AgentTask task);
}