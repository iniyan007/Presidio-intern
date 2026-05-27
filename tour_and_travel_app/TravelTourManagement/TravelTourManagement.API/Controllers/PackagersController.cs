using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.DTOs.Packagers;

namespace TravelTourManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PackagersController : ControllerBase
{
    private readonly IPackagerService _packagerService;

    public PackagersController(IPackagerService packagerService)
    {
        _packagerService = packagerService;
    }

    [HttpPost("apply")]
    [Authorize]
    public async Task<IActionResult> ApplyToBecomePackager([FromBody] ApplyPackagerRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        try
        {
            var response = await _packagerService.ApplyToBecomePackagerAsync(userId, request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while submitting the application.", details = ex.Message });
        }
    }

    [HttpGet("me/status")]
    [Authorize]
    public async Task<IActionResult> GetMyStatus()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        try
        {
            var response = await _packagerService.GetMyPackagerStatusAsync(userId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching your application status.", details = ex.Message });
        }
    }
}
