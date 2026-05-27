using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TravelTourManagement.Business.Providers;
using TravelTourManagement.DataAccess.DTOs.Users;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPackagerRepository _packagerRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly string _adminEmail;

    public AuthService(
        IUserRepository userRepository,
        IPackagerRepository packagerRepository,
        IJwtProvider jwtProvider,
        IEmailService emailService,
        IOtpService otpService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _packagerRepository = packagerRepository;
        _jwtProvider = jwtProvider;
        _emailService = emailService;
        _otpService = otpService;
        _adminEmail = configuration["AdminEmail"] ?? "admin@traveltour.com";
    }

    public async Task<AuthResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            IsActive = true,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);
        
        var role = createdUser.Email.Equals(_adminEmail, StringComparison.OrdinalIgnoreCase) 
            ? "Admin" 
            : "Traveler";

        var token = _jwtProvider.GenerateToken(createdUser.Id, createdUser.Email, role, createdUser.IsEmailVerified);

        return new AuthResponse(
            token,
            new UserResponse(
                createdUser.Id,
                createdUser.FullName,
                createdUser.Email,
                createdUser.Phone,
                createdUser.ProfilePicture,
                createdUser.IsActive,
                createdUser.IsEmailVerified,
                false // Not a packager yet
            )
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is deactivated.");
        }

        // Determine Role
        string role = "Traveler";
        bool isPackager = false;

        if (user.Email.Equals(_adminEmail, StringComparison.OrdinalIgnoreCase))
        {
            role = "Admin";
        }
        else 
        {
            var packager = await _packagerRepository.GetByUserIdAsync(user.Id, cancellationToken);
            if (packager != null && packager.ApprovedAt != null && packager.DeactivatedAt == null)
            {
                role = "Packager";
                isPackager = true;
            }
        }

        // Update last login
        await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

        var token = _jwtProvider.GenerateToken(user.Id, user.Email, role, user.IsEmailVerified);

        return new AuthResponse(
            token,
            new UserResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                user.ProfilePicture,
                user.IsActive,
                user.IsEmailVerified,
                isPackager
            )
        );
    }

    public async Task SendVerificationOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (user.IsEmailVerified)
        {
            throw new InvalidOperationException("Email is already verified.");
        }

        var otp = await _otpService.GenerateAndStoreOtpAsync(email);
        
        var body = $"Hello {user.FullName},\n\nYour email verification code is: {otp}\n\nThis code will expire in 10 minutes.";
        await _emailService.SendEmailAsync(user.Email, "Email Verification OTP", body, cancellationToken);
    }

    public async Task<AuthResponse> VerifyEmailWithOtpAsync(string email, string otp, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (user.IsEmailVerified)
        {
            throw new InvalidOperationException("Email is already verified.");
        }

        var isValid = await _otpService.VerifyOtpAsync(email, otp);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid or expired OTP.");
        }

        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        // Determine Role
        string role = "Traveler";
        bool isPackager = false;
        if (user.Email.Equals(_adminEmail, StringComparison.OrdinalIgnoreCase))
        {
            role = "Admin";
        }
        else 
        {
            var packager = await _packagerRepository.GetByUserIdAsync(user.Id, cancellationToken);
            if (packager != null && packager.ApprovedAt != null && packager.DeactivatedAt == null)
            {
                role = "Packager";
                isPackager = true;
            }
        }

        var token = _jwtProvider.GenerateToken(user.Id, user.Email, role, user.IsEmailVerified);

        return new AuthResponse(
            token,
            new UserResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                user.ProfilePicture,
                user.IsActive,
                user.IsEmailVerified,
                isPackager
            )
        );
    }
}
