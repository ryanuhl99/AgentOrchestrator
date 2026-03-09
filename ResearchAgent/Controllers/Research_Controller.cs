using ResearchAgent.Logic;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace ResearchAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResearchAgentController(ResearchLogic researchLogic) : ControllerBase
{
    private readonly ResearchLogic _researchLogic = researchLogic;
    [HttpPost]
    public async Task<ActionResult<AgentResponse>> ExecuteAgentTask([FromBody] AgentTask request)
    {
        try
        {
            return Ok(await _researchLogic.ExecuteAgentTask(request));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex}");
        }  
    }
}