using Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlannerController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RunPromptResponse>> RunPrompt([FromBody] RunPromptRequest request)
    {
        // do work
        return Ok();
    }
}