
using ReviewAgent.Clients;
using Shared.Models;
namespace ReviewAgent.Logic;

public class ReviewLogic (AgentClient client)
{
    private readonly AgentClient _client = client;
    public async Task<AgentResponse> ExecuteAgentTask(AgentTask task)
    {
        return await _client.CompleteAsync(task);
    }
}