using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.DTOs.Users;
using System.Threading.Tasks;

namespace TravelTourManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("send-otp")]
    [Authorize]
    public async Task<IActionResult> SendOtp()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(email))
            throw new System.UnauthorizedAccessException("Email not found in token.");

        await _authService.SendVerificationOtpAsync(email);
        return Ok(new { message = "OTP sent successfully. Please check your email (or server console logs)." });
    }

    [HttpPost("verify-otp")]
    [Authorize]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(email))
            throw new System.UnauthorizedAccessException("Email not found in token.");

        var response = await _authService.VerifyEmailWithOtpAsync(email, request.Otp);
        return Ok(new { message = "Email verified successfully.", authResponse = response });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] TravelTourManagement.DataAccess.DTOs.Users.RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        return Ok(response);
    }
}
