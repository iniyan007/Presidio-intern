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
    public async Task<IActionResult> CreateBooking([FromForm] CreateBookingCombinedRequest request)
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

        var response = await _bookingService.CreateBookingAsync(userId, bookingData, request.DocumentFiles);
        return Ok(response);
    }

    [HttpPost("{id}/pay")]
    [Authorize(Roles = "Admin,Traveler")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] ProcessPaymentRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _paymentService.ProcessPaymentAsync(userId, id, request);
        return Ok(response);
    }

    [HttpPut("{id}/verify")]
    [Authorize(Roles = "Admin,Packager")]
    public async Task<IActionResult> VerifyBooking(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var response = await _bookingService.VerifyBookingAsync(userId, id);
        var primaryTraveler = response.Travelers?.FirstOrDefault(t => t.IsPrimary) ?? response.Travelers?.FirstOrDefault();
        string fullName = primaryTraveler != null ? primaryTraveler.FullName : "the user";
        
        return Ok(new { message = $"The booking is confirmed for {fullName}." });
    }

    [HttpGet("package/{packageId}")]
    [Authorize(Roles = "Admin,Packager")]
    public async Task<IActionResult> GetBookingsByPackageId(Guid packageId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var response = await _bookingService.GetBookingsByPackageIdAsync(userId, userRole, packageId);
        return Ok(response);
    }
}
