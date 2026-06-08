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

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
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
}
