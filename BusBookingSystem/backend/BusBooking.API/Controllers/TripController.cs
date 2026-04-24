using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BusBooking.Infrastructure.Services;
using BusBooking.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class TripController : ControllerBase
{
    private readonly TripService _service;

    public TripController(TripService service)
    {
        _service = service;
    }

    // Operator creates trip
    [Authorize(Roles = "OPERATOR")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateTripRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.CreateTrip(userId, request);
        return Ok(result);
    }

    // View all trips
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAllTrips());
    }

    // Search trips
    [HttpGet("search")]
    public IActionResult Search(string source, string destination, DateTime? date)
    {
        var result = _service.Search(source, destination, date);
        return Ok(result);
    }

    [Authorize(Roles = "OPERATOR")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateTripRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.UpdateTrip(userId, id, request);
        return Ok(result);
    }
    [Authorize(Roles = "OPERATOR")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.DeleteTrip(userId, id);
        return Ok(result);
    }
    [HttpGet("seats")]
    public IActionResult GetSeats(int tripId)
    {
        var result = _service.GetSeatAvailability(tripId);
        return Ok(result);
    }
}