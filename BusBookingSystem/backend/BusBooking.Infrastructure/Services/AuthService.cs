using BusBooking.Domain.Entities;
using BusBooking.Infrastructure.Data;
using BusBooking.Application.DTOs;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Infrastructure.Services;

public class AuthService

{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Register(RegisterRequest request)
    {
        var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (existingUser != null)
            return "User already exists";

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Age = request.Age,
            Gender = request.Gender,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = 1 // USER
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return "User registered successfully";
    }

    public User ValidateUser(LoginRequest request)
    {
        var user = _context.Users
            .Include(u => u.Role)   // 🔥 THIS IS THE FIX
            .FirstOrDefault(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<string> UpdateProfile(int userId, UpdateProfileRequest request)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return "User not found";

        user.Name = request.Name;
        user.Phone = request.Phone;
        user.Age = request.Age;
        user.Gender = request.Gender;

        await _context.SaveChangesAsync();
        return "Profile updated successfully";
    }
}