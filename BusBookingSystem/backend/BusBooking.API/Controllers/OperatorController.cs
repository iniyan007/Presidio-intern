using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusBooking.Infrastructure.Services;
using BusBooking.Application.DTOs;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class OperatorController : ControllerBase
{
    private readonly OperatorService _service;

    public OperatorController(OperatorService service)
    {
        _service = service;
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
}