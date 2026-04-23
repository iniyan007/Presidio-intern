using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BusBooking.Infrastructure.Services;
using BusBooking.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class BusController : ControllerBase
{
    private readonly BusService _service;

    public BusController(BusService service)
    {
        _service = service;
    }

    // Operator adds bus
    [Authorize(Roles = "OPERATOR")]
    [HttpPost]
    public async Task<IActionResult> AddBus(CreateBusRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.AddBus(userId, request);
        return Ok(result);
    }

    // Admin approves bus
    [Authorize(Roles = "ADMIN")]
    [HttpPut("approve/{id}")]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _service.ApproveBus(id);
        return Ok(result);
    }

    // View buses
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAll());
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("reject/{id}")]
    public async Task<IActionResult> Reject(int id)
    {
        var result = await _service.RejectBus(id);
        return Ok(result);
    }
}