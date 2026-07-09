using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Ai;
using Microsoft.AspNetCore.Authorization;

namespace TravelTourManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null || request.Messages == null || request.Messages.Count == 0)
        {
            return BadRequest(new { success = false, message = "Messages list cannot be empty." });
        }

        var response = await _aiService.GenerateChatResponseAsync(request, cancellationToken);
        
        return Ok(new
        {
            success = true,
            data = response
        });
    }
}
