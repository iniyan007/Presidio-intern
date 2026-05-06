using Microsoft.AspNetCore.Mvc;
using backend.DTOs;

namespace backend.Controllers
{
    public partial class AuthController
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success)
            {
                // Specifically handling the 403 status for pending operators
                if (result.Message.Contains("pending admin approval"))
                {
                    return StatusCode(403, new { message = result.Message });
                }
                return Unauthorized(new { message = result.Message });
            }
            return Ok(result.Data);
        }
    }
}
