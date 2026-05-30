using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTourManagement.Business.Services;

namespace TravelTourManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IPackagerService _packagerService;

    public AdminController(IPackagerService packagerService)
    {
        _packagerService = packagerService;
    }

    [HttpPost("packagers/{id:guid}/approve")]
    public async Task<IActionResult> ApprovePackager(Guid id, CancellationToken cancellationToken)
    {
        var adminUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserIdString) || !Guid.TryParse(adminUserIdString, out var adminUserId))
        {
            return Unauthorized("Admin User ID not found in token.");
        }

        try
        {
            var response = await _packagerService.ApprovePackagerAsync(id, adminUserId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while approving the packager.", details = ex.Message });
        }
    }

    [HttpPost("packagers/{id:guid}/reject")]
    public async Task<IActionResult> RejectPackager(Guid id, [FromBody] TravelTourManagement.DataAccess.DTOs.Packagers.RejectPackagerRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserIdString) || !Guid.TryParse(adminUserIdString, out var adminUserId))
        {
            return Unauthorized("Admin User ID not found in token.");
        }

        try
        {
            var response = await _packagerService.RejectPackagerAsync(id, adminUserId, request.Reason, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while rejecting the packager.", details = ex.Message });
        }
    }

    [HttpGet("packagers/pending")]
    public async Task<IActionResult> GetPendingPackagers(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _packagerService.GetPendingPackagersAsync(cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching pending packagers.", details = ex.Message });
        }
    }
}
