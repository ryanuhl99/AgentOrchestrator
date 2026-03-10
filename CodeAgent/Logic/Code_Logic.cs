
using CodeAgent.Clients;
using Shared.Models;
namespace CodeAgent.Logic;

public class CodeLogic (AgentClient client)
{
    private readonly AgentClient _client = client;
    public async Task<AgentResponse> ExecuteAgentTask(AgentTask task)
    {
        return await _client.CompleteAsync(task);
    }
}