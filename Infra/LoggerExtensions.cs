using Microsoft.Extensions.Logging;

namespace Infra;

public static class LoggerExtensions
{
    public static void LogAgentInfo(
        this ILogger logger,
        string agentName,
        string taskId,
        int inputTokens,
        int outputTokens,
        long latencyMs,
        bool isSuccess
    )
    {
        logger.LogInformation(
            "Agent={AgentName} TaskId={TaskId} InputTokens={InputTokens} OutputTokens={OutputTokens} Latency={LatencyMs} IsSuccess={IsSuccess}",
            agentName,
            taskId,
            inputTokens,
            outputTokens,
            latencyMs,
            isSuccess
        );
    }

    public static void LogAgentError(
        this ILogger logger,
        Exception exception,
        string agentName,
        string taskId,
        int inputTokens,
        int outputTokens,
        long latencyMs,
        bool isSuccess,
        string errorMessage
    )
    {
        logger.LogError(
            exception,
            "Agent={AgentName} TaskId={TaskId} InputTokens={InputTokens} OutputTokens={OutputTokens} Latency={LatencyMs} IsSuccess={IsSuccess}",
            agentName,
            taskId,
            inputTokens,
            outputTokens,
            latencyMs,
            isSuccess
        );
    }
}