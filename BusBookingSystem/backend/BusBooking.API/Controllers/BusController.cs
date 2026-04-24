using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BusBooking.Infrastructure.Services;
using BusBooking.Application.DTOs;
using BusBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class BusController : ControllerBase
{
    private readonly BusService _service;
    private readonly ApplicationDbContext _context; 
    public BusController(BusService service,ApplicationDbContext context)
    {
        _service = service;
        _context = context;
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

    // View all buses (public)
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAll());
    }

    // Operator views their own buses
    [Authorize(Roles = "OPERATOR")]
    [HttpGet("operator")]
    public IActionResult GetOperatorBuses()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var op = _context.Operators
            .FirstOrDefault(o => o.UserId == userId);

        if (op == null)
            return Ok(new List<object>());

        var buses = _context.Buses
            .Where(b => b.OperatorId == op.Id)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.BusNumber,
                b.TotalSeats,
                b.Price,
                b.IsApproved,
                b.IsActive
            }).ToList();

        return Ok(buses);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("reject/{id}")]
    public async Task<IActionResult> Reject(int id)
    {
        var result = await _service.RejectBus(id);
        return Ok(result);
    }
    [Authorize(Roles = "ADMIN")]
    [HttpGet("pending")]
    public IActionResult GetPendingBuses()
    {
        var buses = _context.Buses
            .Include(b => b.Operator)
            .Where(b => !b.IsApproved)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.BusNumber,
                b.TotalSeats,
                b.Price,
                OperatorName = b.Operator.CompanyName
            })
            .ToList();

        return Ok(buses);
    }

    // Operator updates bus
    [Authorize(Roles = "OPERATOR")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBus(int id, CreateBusRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var result = await _service.UpdateBus(userId, id, request);
        return Ok(result);
    }

    // Operator deletes bus
    [Authorize(Roles = "OPERATOR")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBus(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var result = await _service.DeleteBus(userId, id);
        return Ok(result);
    }
}