using ReviewAgent.Logic;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace ReviewAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewAgentController(ReviewLogic reviewLogic) : ControllerBase
{
    private readonly ReviewLogic _reviewLogic = reviewLogic;
    [HttpPost]
    public async Task<ActionResult<AgentResponse>> ExecuteAgentTask([FromBody] AgentTask request)
    {
        try
        {
            return Ok(await _reviewLogic.ExecuteAgentTask(request));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex}");
        }  
    }
}