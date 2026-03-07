using PlannerAgent.Common.Enums;
using PlannerAgent.Common.Models;

namespace PlannerAgent.Services.Clients.Agents;

public class CodeAgentClient : IAgent
{
    public async Task<AgentResponse> ExecuteAsync(AgentTask task)
    {
        return new AgentResponse(
            "",
            false,
            "",
            1,
            1,
            1
        );
    }
}