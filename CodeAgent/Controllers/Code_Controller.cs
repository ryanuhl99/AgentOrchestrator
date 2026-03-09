using CodeAgent.Logic;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CodeAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CodeAgentController(CodeLogic codeLogic) : ControllerBase
{
    private readonly CodeLogic _codeLogic = codeLogic;
    [HttpPost]
    public async Task<ActionResult<AgentResponse>> ExecuteAgentTask([FromBody] AgentTask request)
    {
        try
        {
            return Ok(await _codeLogic.ExecuteAgentTask(request));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex}");
        }  
    }
}