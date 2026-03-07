using Common.Models;
using Common.Enums;
using Services.Clients.Agents;

namespace Common.Utils;

public class AgentResolver
{
    private readonly ResearchAgentClient _researchClient;
    private readonly CodeAgentClient _codeClient;
    private readonly ReviewAgentClient _reviewClient;

    public AgentResolver(
        ResearchAgentClient researchClient,
        CodeAgentClient codeClient,
        ReviewAgentClient reviewClient
    )
    {
        _researchClient = researchClient;
        _codeClient = codeClient;
        _reviewClient = reviewClient;
    }

    public IAgent Resolve(AgentTypeEnum type) =>
    type switch
    {
        AgentTypeEnum.ResearchAgent => new ResearchAgentClient(),
        AgentTypeEnum.CodeAgent => new CodeAgentClient(),
        AgentTypeEnum.ReviewAgent => new ReviewAgentClient(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}