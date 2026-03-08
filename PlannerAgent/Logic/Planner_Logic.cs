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
        try
        {
            var initLlmResponse = await _llmService.GetLlmCompletionContent(request);
            
            return new RunPromptResponse();
        }
        // build graph
        // schedule calls
        // format prompt response
    }
}