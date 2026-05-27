using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.DTOs.Users;

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration.", details = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login.", details = ex.Message });
        }
    }

    [HttpPost("send-otp")]
    [Authorize]
    public async Task<IActionResult> SendOtp()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { message = "Email not found in token." });

        try
        {
            await _authService.SendVerificationOtpAsync(email);
            return Ok(new { message = "OTP sent successfully. Please check your email (or server console logs)." });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
        }
    }

    [HttpPost("verify-otp")]
    [Authorize]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { message = "Email not found in token." });

        try
        {
            var response = await _authService.VerifyEmailWithOtpAsync(email, request.Otp);
            return Ok(new { message = "Email verified successfully.", authResponse = response });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
        }
    }
}
