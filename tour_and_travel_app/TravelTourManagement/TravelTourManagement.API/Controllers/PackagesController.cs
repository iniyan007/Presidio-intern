using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Packages;

namespace TravelTourManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PackagesController : ControllerBase
{
    private readonly IPackageService _packageService;

    public PackagesController(IPackageService packageService)
    {
        _packageService = packageService;
    }

    [HttpPost]
    [Authorize(Roles = "Packager")]
    public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        try
        {
            var packageId = await _packageService.CreatePackageAsync(userId, request);
            return CreatedAtAction(nameof(CreatePackage), new { id = packageId }, new { id = packageId, message = "Package created successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the package.", details = ex.Message });
        }
    }
}
