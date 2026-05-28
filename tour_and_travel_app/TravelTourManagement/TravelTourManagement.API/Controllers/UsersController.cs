using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Users;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TravelTourManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly string _uploadDirectory;

    public UsersController(IUserService userService)
    {
        _userService = userService;
        var currentDirectory = Directory.GetCurrentDirectory(); 
        var solutionDirectory = Directory.GetParent(currentDirectory)?.FullName ?? currentDirectory;
        _uploadDirectory = Path.Combine(solutionDirectory, "TravelTourManagement.DataAccess", "Uploads", "ProfilePictures");
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _userService.GetProfileAsync(userId);
        return Ok(response);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _userService.UpdateProfileAsync(userId, request);
        return Ok(response);
    }

    [HttpPost("profile/picture")]
    [Authorize]
    public async Task<IActionResult> UploadProfilePicture(Microsoft.AspNetCore.Http.IFormFile profilePicture)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        if (profilePicture == null || profilePicture.Length == 0)
            throw new ArgumentException("No file uploaded.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
        if (!System.Linq.Enumerable.Contains(allowedExtensions, extension))
            throw new ArgumentException("Invalid file type. Only JPG, JPEG, and PNG are allowed.");

        if (profilePicture.Length > 5 * 1024 * 1024) // 5 MB
            throw new ArgumentException("File size exceeds 5MB limit.");

        using var stream = profilePicture.OpenReadStream();
        var response = await _userService.UploadProfilePictureAsync(userId, stream, profilePicture.FileName, profilePicture.ContentType);
        return Ok(response);
    }

    [HttpDelete("profile/picture")]
    [Authorize]
    public async Task<IActionResult> RemoveProfilePicture()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _userService.RemoveProfilePictureAsync(userId);
        return Ok(response);
    }

    [HttpGet("profile/picture/{fileName}")]
    [AllowAnonymous]
    public IActionResult GetProfilePicture(string fileName)
    {
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            throw new ArgumentException("Invalid file name.");

        var filePath = Path.Combine(_uploadDirectory, fileName);
        if (!System.IO.File.Exists(filePath))
            throw new System.Collections.Generic.KeyNotFoundException("File not found.");

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, contentType);
    }
}
