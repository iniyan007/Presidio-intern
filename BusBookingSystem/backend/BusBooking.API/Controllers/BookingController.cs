using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BusBooking.Infrastructure.Services;
using BusBooking.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly BookingService _service;

    public BookingController(BookingService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost("lock")]
    public async Task<IActionResult> Lock(int tripId, int seatId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.LockSeat(userId, tripId, seatId);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm(int tripId, int seatId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.ConfirmBooking(userId, tripId, seatId);
        return Ok(result);
    }
    [Authorize]
    [HttpPut("cancel/{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.CancelBooking(userId, id);
        return Ok(result);
    }
    [Authorize]
    [HttpGet("my-bookings")]
    public IActionResult MyBookings()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        return Ok(_service.GetUserBookings(userId));
    }
    [Authorize(Roles = "OPERATOR")]
    [HttpGet("revenue")]
    public IActionResult GetRevenue()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var revenue = _service.GetOperatorRevenue(userId);

        return Ok(new { revenue });
    }
    [Authorize(Roles = "ADMIN")]
    [HttpGet("total-revenue")]
    public IActionResult TotalRevenue()
    {
        return Ok(new { revenue = _service.GetTotalRevenue() });
    }
}