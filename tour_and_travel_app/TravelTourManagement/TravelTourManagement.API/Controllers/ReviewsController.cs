using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Reviews;
using System;
using System.Threading;

namespace TravelTourManagement.API.Controllers;

[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly IBlobStorageService _blobStorageService;

    public ReviewsController(IReviewService reviewService, IBlobStorageService blobStorageService)
    {
        _reviewService = reviewService;
        _blobStorageService = blobStorageService;
    }

    [HttpPost("api/Bookings/{bookingId}/reviews")]
    [Authorize(Roles = "Traveler")]
    [TypeFilter(typeof(TravelTourManagement.API.Filters.IdempotentAttribute))]
    public async Task<IActionResult> CreateReview(Guid bookingId, [FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");
        var finalRequest = request with { BookingId = bookingId };

        var review = await _reviewService.CreateReviewAsync(userId, finalRequest, cancellationToken);
        return Ok(new { success = true, message = "Review submitted successfully.", data = review });
    }

    [HttpGet("api/Packages/{packageId}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPackageReviews(Guid packageId, CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetReviewsByPackageIdAsync(packageId, cancellationToken);
        return Ok(reviews);
    }

    [HttpGet("api/Packagers/{packagerId}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPackagerReviews(Guid packagerId, CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetReviewsByPackagerIdAsync(packagerId, cancellationToken);
        return Ok(reviews);
    }

    [HttpPost("api/Reviews/upload-media")]
    [Authorize(Roles = "Traveler")]
    public async Task<IActionResult> UploadMedia(List<Microsoft.AspNetCore.Http.IFormFile> files, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        if (files == null || files.Count == 0)
            return BadRequest(new { message = "No files uploaded." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var uploadedPaths = new List<string>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!System.Linq.Enumerable.Contains(allowedExtensions, extension))
                return BadRequest(new { message = $"Invalid file type for {file.FileName}. Only JPG, JPEG, and PNG are allowed." });

            if (file.Length > 5 * 1024 * 1024) // 5 MB limit per file
                return BadRequest(new { message = $"File {file.FileName} exceeds 5MB limit." });

            using var stream = file.OpenReadStream();
            string fileUrl = await _blobStorageService.UploadFileAsync(stream, file.FileName, file.ContentType, "web-images", cancellationToken);

            uploadedPaths.Add(fileUrl);
        }

        return Ok(new { success = true, paths = uploadedPaths });
    }

    [HttpGet("api/Reviews/media/{fileName}")]
    [AllowAnonymous]
    public IActionResult GetMedia(string fileName)
    {
        // Media is now served directly from Azure Blob Storage.
        // This endpoint is kept for backwards compatibility with older reviews if necessary,
        // but new reviews will use direct blob URLs.
        return NotFound("File not found locally. It might be in blob storage.");
    }
}
