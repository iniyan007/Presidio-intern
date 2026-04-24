using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _service;

    public PaymentController(PaymentService service)
    {
        _service = service;
    }

    [Authorize(Roles = "USER")]
    [HttpPost("pay")]
    public async Task<IActionResult> Pay(int bookingId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.ProcessPayment(userId, bookingId);

        if (result.Contains("failed"))
            return BadRequest(new { message = result });

        return Ok(new { message = result });
    }
}