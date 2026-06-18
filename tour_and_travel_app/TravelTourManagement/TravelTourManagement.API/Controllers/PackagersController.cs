using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.DTOs.Packagers;
using System;
using System.Threading.Tasks;

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
    public async Task<IActionResult> ApplyToBecomePackager([FromForm] ApplyPackagerRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _packagerService.ApplyToBecomePackagerAsync(userId, request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me/status")]
    [Authorize]
    public async Task<IActionResult> GetMyStatus(CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _packagerService.GetMyPackagerStatusAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicPackagers([FromQuery] PackagerSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await _packagerService.GetPublicPackagersAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("public/{name}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicPackagerByName(string name, CancellationToken cancellationToken)
    {
        var response = await _packagerService.GetPublicPackagerByNameAsync(name, cancellationToken);
        return Ok(response);
    }
}
