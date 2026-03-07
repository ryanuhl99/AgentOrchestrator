using Common.Enums;
using Common.Models;

namespace Services.Clients.Agents;

public class ResearchAgentClient : IAgent
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