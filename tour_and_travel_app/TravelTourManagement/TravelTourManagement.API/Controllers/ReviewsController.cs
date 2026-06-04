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

    /// <summary>
    /// Submits a new review for a previously completed booking.
    /// Only the traveler who owns the booking can submit this.
    /// </summary>
    [HttpPost("api/Bookings/{bookingId}/reviews")]
    [Authorize(Roles = "Traveler")]
    [TypeFilter(typeof(TravelTourManagement.API.Filters.IdempotentAttribute))]
    public async Task<IActionResult> CreateReview(Guid bookingId, [FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        // CreateReviewRequest already has BookingId inside it technically, but route param is cleaner.
        // We will pass the route param directly or override the request's BookingId.
        // Wait, CreateReviewRequest is a record with BookingId in it, so the model binder expects it.
        // We can just construct a new request or use the one from body.
        
        // Ensure the route matches the body if they send both.
        // Actually, looking at the CreateReviewRequest we found earlier:
        // public record CreateReviewRequest(Guid BookingId, ...);
        // It's better to use the one from the body, or create a unified object.
        // Let's rely on the Request body being populated correctly, but force the bookingId to match the route.
        var finalRequest = request with { BookingId = bookingId };

        var review = await _reviewService.CreateReviewAsync(userId, finalRequest, cancellationToken);
        return Ok(new { success = true, message = "Review submitted successfully.", data = review });
    }

    /// <summary>
    /// Retrieves all reviews for a specific package.
    /// Accessible to anyone (public).
    /// </summary>
    [HttpGet("api/Packages/{packageId}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPackageReviews(Guid packageId, CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetReviewsByPackageIdAsync(packageId, cancellationToken);
        return Ok(reviews);
    }

    /// <summary>
    /// Retrieves all reviews for an agency/packager.
    /// </summary>
    [HttpGet("api/Packagers/{packagerId}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPackagerReviews(Guid packagerId, CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetReviewsByPackagerIdAsync(packagerId, cancellationToken);
        return Ok(reviews);
    }
}
