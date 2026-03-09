
namespace PlannerAgent.Common.Models;

public class AgentResponse
{
    public string TaskId { get; set; } = "";
    public bool IsSuccess { get; set; }
    public string Output { get; set; } = "";
    public string? Error { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public long LatencyMs { get; set; }
}