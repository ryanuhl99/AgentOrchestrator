using Common.Enums;
using Common.Models;

namespace Services.Clients.Agents;

public interface IAgent
{
    Task<AgentResponse> ExecuteAsync(AgentTask task);
}