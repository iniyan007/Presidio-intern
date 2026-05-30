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
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("send-otp")]
    [Authorize]
    public async Task<IActionResult> SendOtp(CancellationToken cancellationToken)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(email))
            throw new System.UnauthorizedAccessException("Email not found in token.");

        await _authService.SendVerificationOtpAsync(email, cancellationToken);
        return Ok(new { message = "OTP sent successfully. Please check your email (or server console logs)." });
    }

    [HttpPost("verify-otp")]
    [Authorize]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(email))
            throw new System.UnauthorizedAccessException("Email not found in token.");

        var response = await _authService.VerifyEmailWithOtpAsync(email, request.Otp, cancellationToken);
        return Ok(new { message = "Email verified successfully.", authResponse = response });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] TravelTourManagement.DataAccess.DTOs.Users.RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(response);
    }
}
