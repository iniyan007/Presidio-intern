using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs;

namespace backend.Controllers
{
    public partial class AuthController
    {
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var result = await _authService.UpdateProfileAsync(userId, request);

            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(new { message = result.Message, user = result.User });
        }
    }
}
