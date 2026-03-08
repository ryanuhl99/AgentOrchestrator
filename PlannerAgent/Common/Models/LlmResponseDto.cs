
namespace PlannerAgent.Common.Models;

public class LlmResponseDto
{
    public string Json { get; set; } = "";
    public OpenAiLlmResponse? SuccessResponse { get; set; }
    public OpenAiErrorResponse? FailResponse { get; set; }
    public long Latency { get; set; }
}