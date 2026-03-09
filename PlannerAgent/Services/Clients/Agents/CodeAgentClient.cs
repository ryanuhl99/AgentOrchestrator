
namespace PlannerAgent.Services.Clients.Agents;

public class CodeAgentClient(HttpClient client, ILogger<CodeAgentClient> logger)
    : AgentClientBase(client, logger)
{
    protected override string AgentName => "CodeAgent";
    protected override string Endpoint => "api/codeagent";
}