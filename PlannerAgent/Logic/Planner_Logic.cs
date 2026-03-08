using PlannerAgent.Common.Models;
using PlannerAgent.Services;
using PlannerAgent.Common.Utils;

namespace PlannerAgent.Logic;

public class PlannerLogic (
    LlmService llmService,
    PlannerService plannerService
)
{
    private readonly LlmService _llmService = llmService;
    private readonly PlannerService _plannerService = plannerService;

    public async Task<RunPromptResponse> RunPrompt(RunPromptRequest request)
    {
        // call llm
        // build graph
        // schedule calls
        // format prompt response
        return new RunPromptResponse();
    }
}