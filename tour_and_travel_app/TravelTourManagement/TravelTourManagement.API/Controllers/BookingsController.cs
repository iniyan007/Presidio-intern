using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Bookings;
using System.Linq;

namespace TravelTourManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IPaymentService _paymentService;

    public BookingsController(IBookingService bookingService, IPaymentService paymentService)
    {
        _bookingService = bookingService;
        _paymentService = paymentService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Traveler")]
    [TypeFilter(typeof(TravelTourManagement.API.Filters.IdempotentAttribute))]
    public async Task<IActionResult> CreateBooking([FromForm] CreateBookingCombinedRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        CreateBookingRequest bookingData;
        try
        {
            bookingData = System.Text.Json.JsonSerializer.Deserialize<CreateBookingRequest>(request.BookingData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            
            // Manual validation
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(bookingData, serviceProvider: null, items: null);
            var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(bookingData, context, results, true);
            if (!isValid)
            {
                return BadRequest(results);
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            return BadRequest(new { message = "Invalid JSON in BookingData.", details = ex.Message });
        }

        var response = await _bookingService.CreateBookingAsync(userId, bookingData, request.DocumentFiles, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id}/pay")]
    [Authorize(Roles = "Admin,Traveler")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] ProcessPaymentRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _paymentService.ProcessPaymentAsync(userId, id, request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id}/verify")]
    [Authorize(Roles = "Admin,Packager")]
    public async Task<IActionResult> VerifyBooking(Guid id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _bookingService.VerifyBookingAsync(userId, id, cancellationToken);
        var primaryTraveler = response.Travelers?.FirstOrDefault(t => t.IsPrimary) ?? response.Travelers?.FirstOrDefault();
        string fullName = primaryTraveler != null ? primaryTraveler.FullName : "the user";
        
        return Ok(new { message = $"The booking is confirmed for {fullName}." });
    }

    [HttpGet("my-bookings")]
    [Authorize(Roles = "Traveler,Admin")]
    public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _bookingService.GetMyBookingsAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("package/{packageId}")]
    [Authorize(Roles = "Admin,Packager")]
    public async Task<IActionResult> GetBookingsByPackageId(Guid packageId, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var response = await _bookingService.GetBookingsByPackageIdAsync(userId, userRole, packageId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("documents/{documentId}/verify")]
    [Authorize(Roles = "Packager")]
    public async Task<IActionResult> VerifyDocument(Guid documentId, [FromBody] VerifyDocumentRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _bookingService.VerifyDocumentAsync(userId, documentId, request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("documents/{documentId}/reupload")]
    [Authorize(Roles = "Traveler,Admin")]
    public async Task<IActionResult> ReuploadDocument(Guid documentId, IFormFile file, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _bookingService.ReuploadDocumentAsync(userId, documentId, file, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Traveler,Admin")]
    [TypeFilter(typeof(TravelTourManagement.API.Filters.IdempotentAttribute))]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] TravelTourManagement.DataAccess.DTOs.Bookings.CancelBookingRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        await _bookingService.CancelBookingAsync(userId, id, request, cancellationToken);
        return Ok(new { success = true, message = "Booking cancelled successfully. The amount will be refunded based on the cancellation policy." });
    }

    [HttpGet("{id}/ticket")]
    [Authorize(Roles = "Traveler,Admin")]
    public async Task<IActionResult> DownloadTicket(Guid id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var pdfBytes = await _bookingService.DownloadBookingTicketAsync(userId, id, cancellationToken);
        return File(pdfBytes, "application/pdf", $"BookingTicket-{id}.pdf");
    }
}
