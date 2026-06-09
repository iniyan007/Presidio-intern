using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.PlatformConfig;

namespace TravelTourManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlatformConfigController : ControllerBase
{
    private readonly IPlatformConfigService _platformConfigService;

    public PlatformConfigController(IPlatformConfigService platformConfigService)
    {
        _platformConfigService = platformConfigService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetConfig(CancellationToken cancellationToken)
    {
        var response = await _platformConfigService.GetConfigAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdatePlatformConfigRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var adminUserId))
            return Unauthorized("User ID not found in token.");

        var response = await _platformConfigService.UpdateConfigAsync(adminUserId, request, cancellationToken);
        return Ok(response);
    }
}
