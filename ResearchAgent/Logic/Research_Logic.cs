
using ResearchAgent.Clients;
using Shared.Models;
namespace ResearchAgent.Logic;

public class ResearchLogic (AgentClient client)
{
    private readonly AgentClient _client = client;
    public async Task<AgentResponse> ExecuteAgentTask(AgentTask task)
    {
        return await _client.CompleteAsync(task);
    }
}