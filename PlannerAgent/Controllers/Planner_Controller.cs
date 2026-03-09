using PlannerAgent.Common.Models;
using Microsoft.AspNetCore.Mvc;
using PlannerAgent.Logic;

namespace PlannerAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlannerAgentController(PlannerLogic plannerLogic) : ControllerBase
{
    private readonly PlannerLogic _plannerLogic = plannerLogic;
    [HttpPost]
    public async Task<ActionResult<RunPromptResponse>> RunPrompt([FromBody] RunPromptRequest request)
    {
        try
        {
            var response = await _plannerLogic.RunPrompt(request);
            
            if (!string.IsNullOrWhiteSpace(response.Error))
                throw new Exception($"{response.Error}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex}");
        }
        
    }
}