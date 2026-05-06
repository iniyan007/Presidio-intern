using Microsoft.AspNetCore.Mvc;
using backend.DTOs;

namespace backend.Controllers
{
    public partial class AuthController
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(new { message = result.Message });
        }
    }
}
