
namespace PlannerAgent.Services.Clients.Agents;

public class ResearchAgentClient(HttpClient client, ILogger<ResearchAgentClient> logger)
    : AgentClientBase(client, logger)
{
    protected override string AgentName => "ResearchAgent";
    protected override string Endpoint => "api/researchagent";
}