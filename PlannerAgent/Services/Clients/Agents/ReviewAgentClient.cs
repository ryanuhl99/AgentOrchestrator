
namespace PlannerAgent.Services.Clients.Agents;

public class ReviewAgentClient(HttpClient client, ILogger<ReviewAgentClient> logger)
    : AgentClientBase(client, logger)
{
    protected override string AgentName => "ReviewAgent";
    protected override string Endpoint => "api/reviewagent";
}