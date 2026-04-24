using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusBooking.Infrastructure.Services;
using BusBooking.Application.DTOs;
using System.Security.Claims;
using BusBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class OperatorController : ControllerBase
{
    private readonly OperatorService _service;
    private readonly ApplicationDbContext _context; 
    public OperatorController(OperatorService service,ApplicationDbContext context)
    {
        _service = service;
        _context = context;
    }

    // 👤 USER → Register as operator
    [Authorize]
    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateOperatorRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var result = await _service.RegisterOperator(userId, request);
        return Ok(result);
    }

    // 🧑‍💼 ADMIN → Approve operator
    [Authorize(Roles = "ADMIN")]
    [HttpPut("approve/{id}")]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _service.ApproveOperator(id);
        return Ok(result);
    }

    // ADMIN → view operators
    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAll());
    }
    [Authorize(Roles = "ADMIN")]
    [HttpPut("reject/{id}")]
    public async Task<IActionResult> Reject(int id)
    {
        var result = await _service.RejectOperator(id);
        return Ok(result);
    }
    [Authorize(Roles = "ADMIN")]
    [HttpGet("pending")]
    public IActionResult GetPendingOperators()
    {
        var operators = _context.Operators
            .Include(o => o.User)
            .Where(o => !o.IsApproved)
            .Select(o => new
            {
                o.Id,
                o.CompanyName,
                o.ContactNumber,
                o.OperatingLocation,
                UserName = o.User.Name,
                Email = o.User.Email
            })
            .ToList();

        return Ok(operators);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("disable/{id}")]
    public async Task<IActionResult> Disable(int id, [FromServices] TripService tripService)
    {
        var result = await _service.DisableOperator(id, tripService);
        return Ok(result);
    }
}