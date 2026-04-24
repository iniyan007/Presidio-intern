using Microsoft.AspNetCore.Mvc;
using BusBooking.Application.DTOs;
using BusBooking.Infrastructure.Services;
using BusBooking.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;

    public AuthController(AuthService authService, JwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.Register(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var user = _authService.ValidateUser(request);

        if (user == null)
            return Unauthorized("Invalid credentials");

        var token = _jwtService.GenerateToken(user);

        return Ok(new 
        { 
            token, 
            user = new 
            { 
                id = user.Id, 
                name = user.Name, 
                email = user.Email, 
                role = user.Role?.Name,
                phone = user.Phone,
                age = user.Age,
                gender = user.Gender
            } 
        });
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var result = await _authService.UpdateProfile(userId, request);
        return Ok(result);
    }
}