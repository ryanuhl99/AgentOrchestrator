
namespace PlannerAgent.Common.Models;

public class LlmResponseDto
{
    public AgentGraph Graph { get; set; } = new();
    public OpenAiLlmResponse? SuccessResponse { get; set; }
    public OpenAiErrorResponse? FailResponse { get; set; }
    public long Latency { get; set; }
    public string UserPrompt { get; set; } = "";
}