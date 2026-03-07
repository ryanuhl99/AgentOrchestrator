using Common.Models;
using Services;
using Common.Utils;

namespace Logic;

public class PlannerLogic (
    LlmService llmService,
    GraphBuilder graphBuilder,
    PlannerService plannerService
)
{
    private readonly LlmService _llmService = llmService;
    private readonly GraphBuilder _graphBuilder = graphBuilder;
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