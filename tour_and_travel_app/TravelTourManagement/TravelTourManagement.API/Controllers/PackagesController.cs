using System.Collections.Generic;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Packages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchPackages([FromQuery] PackageSearchRequest request)
    {
        var result = await _packageService.SearchPackagesAsync(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Traveler,Packager")]
    public async Task<IActionResult> GetPublishedPackageById(Guid id)
    {
        var package = await _packageService.GetPublishedPackageByIdAsync(id);
        return Ok(package);
    }

    [HttpPost]
    [Authorize(Roles = "Packager")]
    public async Task<IActionResult> CreatePackage([FromForm] CreatePackageCombinedRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        CreatePackageRequest packageData;
        try
        {
            packageData = System.Text.Json.JsonSerializer.Deserialize<CreatePackageRequest>(request.PackageData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            
            // Manual validation
            var context = new ValidationContext(packageData, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(packageData, context, results, true);
            if (!isValid)
            {
                return BadRequest(results);
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            return BadRequest(new { message = "Invalid JSON in PackageData.", details = ex.Message });
        }

        var packageId = await _packageService.CreatePackageAsync(userId, packageData, request.MediaFiles);
        return CreatedAtAction(nameof(CreatePackage), new { id = packageId }, new { id = packageId, message = "Package created successfully." });
    }



    [HttpGet("media/{fileName}")]
    [AllowAnonymous]
    public IActionResult GetPackageMedia(string fileName)
    {
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            throw new ArgumentException("Invalid file name.");

        var currentDirectory = System.IO.Directory.GetCurrentDirectory(); 
        var solutionDirectory = System.IO.Directory.GetParent(currentDirectory)?.FullName ?? currentDirectory;
        var uploadDirectory = System.IO.Path.Combine(solutionDirectory, "TravelTourManagement.DataAccess", "Uploads", "Packages");
        
        var filePath = System.IO.Path.Combine(uploadDirectory, fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        return File(fileStream, contentType);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Packager")]
    public async Task<IActionResult> DeletePackage(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        await _packageService.DeletePackageAsync(userId, id);
        return Ok(new { message = "Package deleted successfully." });
    }

    [HttpGet("{id}/revenue")]
    [Authorize(Roles = "Admin,Packager")]
    public async Task<IActionResult> GetPackageRevenue(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(role))
            throw new UnauthorizedAccessException("Role not found in token.");

        var result = await _packageService.GetPackageRevenueAsync(userId, role, id);
        return Ok(result);
    }
}