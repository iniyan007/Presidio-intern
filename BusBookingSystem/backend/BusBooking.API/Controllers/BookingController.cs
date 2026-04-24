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
    public async Task<IActionResult> Lock([FromBody] LockSeatRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.LockSeats(userId, request.TripId, request.SeatIds);

        if (result.Contains("not") || result.Contains("already"))
            return BadRequest(new { message = result });

        return Ok(new { message = result });
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.CreateBooking(userId, request.TripId, request.SeatIds);

        if (result.Contains("not") || result.Contains("already"))
            return BadRequest(new { message = result });

        return Ok(new { message = result });
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
    
    [Authorize(Roles = "OPERATOR")]
    [HttpGet("operator-bookings")]
    public IActionResult GetOperatorBookings([FromQuery] int? tripId, int page = 1, int pageSize = 10)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = _service.GetOperatorBookings(userId, page, pageSize, tripId);

        return Ok(result);
    }
}