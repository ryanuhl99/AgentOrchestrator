using Shared.Models;
using Shared.Enums;
using PlannerAgent.Services.Clients.Agents;

namespace PlannerAgent.Common.Utils;

public class AgentResolver(IServiceProvider sp)
{
    private readonly IServiceProvider _sp = sp;

    public IAgent Resolve(AgentTypeEnum type) =>
    type switch
    {
        AgentTypeEnum.ResearchAgent => _sp.GetRequiredService<ResearchAgentClient>(),
        AgentTypeEnum.CodeAgent => _sp.GetRequiredService<CodeAgentClient>(),
        AgentTypeEnum.ReviewAgent => _sp.GetRequiredService<ReviewAgentClient>(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}