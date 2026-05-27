using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Users;
using Microsoft.AspNetCore.StaticFiles;

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
            return Unauthorized("User ID not found in token.");

        try
        {
            var response = await _userService.GetProfileAsync(userId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized("User ID not found in token.");

        try
        {
            var response = await _userService.UpdateProfileAsync(userId, request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("profile/picture")]
    [Authorize]
    public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized("User ID not found in token.");

        if (profilePicture == null || profilePicture.Length == 0)
            return BadRequest("No file uploaded.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest("Invalid file type. Only JPG, JPEG, and PNG are allowed.");

        if (profilePicture.Length > 5 * 1024 * 1024) // 5 MB
            return BadRequest("File size exceeds 5MB limit.");

        try
        {
            using var stream = profilePicture.OpenReadStream();
            var response = await _userService.UploadProfilePictureAsync(userId, stream, profilePicture.FileName, profilePicture.ContentType);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("profile/picture")]
    [Authorize]
    public async Task<IActionResult> RemoveProfilePicture()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized("User ID not found in token.");

        try
        {
            var response = await _userService.RemoveProfilePictureAsync(userId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("profile/picture/{fileName}")]
    [AllowAnonymous]
    public IActionResult GetProfilePicture(string fileName)
    {
        // Prevent directory traversal
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            return BadRequest("Invalid file name.");

        var filePath = Path.Combine(_uploadDirectory, fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, contentType);
    }
}
