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

public class UploadPackageMediaRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
    [Required]
    public string Category { get; set; } = null!;
    [Required]
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? Caption { get; set; }
}

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
    public async Task<IActionResult> GetAllPublishedPackages()
    {
        try
        {
            var packages = await _packageService.GetAllPublishedPackagesAsync();
            return Ok(packages);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving packages.", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Traveler,Packager")]
    public async Task<IActionResult> GetPublishedPackageById(Guid id)
    {
        try
        {
            var package = await _packageService.GetPublishedPackageByIdAsync(id);
            return Ok(package);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the package.", details = ex.Message });
        }
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

    [HttpPost("{id}/media")]
    [Authorize(Roles = "Packager")]
    public async Task<IActionResult> UploadPackageMedia(Guid id, [FromForm] UploadPackageMediaRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = System.IO.Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!System.Linq.Enumerable.Contains(allowedExtensions, extension))
        {
            return BadRequest("Invalid file type. Only JPG, JPEG, and PNG are allowed.");
        }

        if (request.File.Length > 5 * 1024 * 1024)
        {
            return BadRequest("File size exceeds 5MB limit.");
        }

        try
        {
            using var stream = request.File.OpenReadStream();
            var fileName = await _packageService.UploadPackageMediaAsync(userId, id, stream, request.File.FileName, request.File.ContentType, request.Category, request.IsPrimary, request.DisplayOrder, request.Caption);
            return Ok(new { message = "Package media uploaded successfully.", fileName = fileName });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while uploading package media.", details = ex.Message });
        }
    }

    [HttpGet("media/{fileName}")]
    [AllowAnonymous]
    public IActionResult GetPackageMedia(string fileName)
    {
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            return BadRequest("Invalid file name.");

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
        {
            return Unauthorized("User ID not found in token.");
        }

        try
        {
            await _packageService.DeletePackageAsync(userId, id);
            return Ok(new { message = "Package deleted successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the package.", details = ex.Message });
        }
    }
}
