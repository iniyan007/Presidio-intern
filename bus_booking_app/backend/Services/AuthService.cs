using backend.Interfaces;
using backend.Data;
using backend.Models;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return (false, "Email already in use");
            }

            User user;
            if (request.Role == "Operator")
            {
                user = new Operator
                {
                    Name = request.Name, Email = request.Email, MobileNumber = request.MobileNumber, 
                    Age = request.Age, Gender = request.Gender, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    IsApproved = false
                };
            }
            else if (request.Role == "Admin")
            {
                user = new Admin
                {
                    Name = request.Name, Email = request.Email, MobileNumber = request.MobileNumber, 
                    Age = request.Age, Gender = request.Gender, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                };
            }
            else
            {
                user = new Customer
                {
                    Name = request.Name, Email = request.Email, MobileNumber = request.MobileNumber, 
                    Age = request.Age, Gender = request.Gender, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                };
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "Registration successful");
        }

        public async Task<(bool Success, string Message, object? Data)> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return (false, "Invalid email or password", null);
            }

            if (user is Operator op && !op.IsApproved)
            {
                return (false, "Your account is pending admin approval. Please wait until an admin approves your operator account.", null);
            }

            var token = GenerateJwtToken(user);

            var data = new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role
                }
            };

            return (true, "Login successful", data);
        }

        public async Task<(bool Success, string Message, object? User)> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (false, "User not found", null);

            user.Name = request.Name;
            user.MobileNumber = request.MobileNumber;
            user.Age = request.Age;
            user.Gender = request.Gender;

            await _context.SaveChangesAsync();

            var updatedUser = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.MobileNumber,
                user.Age,
                user.Gender
            };

            return (true, "Profile updated successfully", updatedUser);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
