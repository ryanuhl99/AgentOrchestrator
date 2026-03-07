
namespace PlannerAgent.Common.Models;

public class AgentResponse (
    string taskId,
    bool isSuccess,
    string output,
    int inputTokens,
    int outputTokens,
    long latencyMs
)
{
    public string TaskId { get; set; } = taskId;
    public bool IsSuccess { get; set; } = isSuccess;
    public string Output { get; set; } = output;
    public string? Error { get; set; }
    public int InputTokens { get; set; } = inputTokens;
    public int OutputTokens { get; set; } = outputTokens;
    public long LatencyMs { get; set; } = latencyMs;
}