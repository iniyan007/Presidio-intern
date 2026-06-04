using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TravelTourManagement.Business.Providers;
using TravelTourManagement.DataAccess.DTOs.Users;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;
using AutoMapper;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPackagerRepository _packagerRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly IOtpService _otpService;
    private readonly string _adminEmail;

    public AuthService(
        IUserRepository userRepository,
        IPackagerRepository packagerRepository,
        IJwtProvider jwtProvider,
        IEmailService emailService,
        IOtpService otpService,
        IConfiguration configuration,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _packagerRepository = packagerRepository;
        _jwtProvider = jwtProvider;
        _emailService = emailService;
        _mapper = mapper;
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
        var refreshToken = _jwtProvider.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user, cancellationToken);


        return new AuthResponse(
            token,
            refreshToken,
            _mapper.Map<UserResponse>(user) 
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
                            }
        }

        // Update last login
        await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

        var token = _jwtProvider.GenerateToken(user.Id, user.Email, role, user.IsEmailVerified);
        var refreshToken = _jwtProvider.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user, cancellationToken);


        return new AuthResponse(
            token,
            refreshToken,
            _mapper.Map<UserResponse>(user) 
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
            throw new ArgumentException("Invalid or expired OTP.");
        }

        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        // Determine Role
        string role = "Traveler";
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
            }
        }

        var token = _jwtProvider.GenerateToken(user.Id, user.Email, role, user.IsEmailVerified);
        var refreshToken = _jwtProvider.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user, cancellationToken);


        return new AuthResponse(
            token,
            refreshToken,
            _mapper.Map<UserResponse>(user) 
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtProvider.GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
            throw new UnauthorizedAccessException("Invalid access token or refresh token.");

        var userIdString = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("Invalid token claims.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        // Determine Role
        string role = "Traveler";
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
            }
        }

        var newAccessToken = _jwtProvider.GenerateToken(user.Id, user.Email, role, user.IsEmailVerified);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new AuthResponse(
            newAccessToken,
            newRefreshToken,
            _mapper.Map<UserResponse>(user) 
        );
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            // To prevent email enumeration, we do not throw an error if the email is not found.
            return;
        }

        var otp = await _otpService.GenerateAndStoreOtpAsync(email);
        
        var body = $"Hello {user.FullName},\n\nYour password reset OTP is: {otp}\n\nThis code will expire in 10 minutes.";
        await _emailService.SendEmailAsync(user.Email, "Password Reset OTP", body, cancellationToken);
    }

    public async Task<string> VerifyResetOtpAsync(VerifyResetOtpRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            throw new ArgumentException("Invalid or expired OTP.");
        }

        var isValid = await _otpService.VerifyOtpAsync(request.Email, request.Otp);
        if (!isValid)
        {
            throw new ArgumentException("Invalid or expired OTP.");
        }

        // Generate a temporary reset token that expires in 15 minutes
        var resetToken = await _otpService.GenerateAndStoreResetTokenAsync(request.Email);
        return resetToken;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            throw new ArgumentException("Invalid reset token.");
        }

        var isValid = await _otpService.VerifyResetTokenAsync(request.Email, request.ResetToken);
        if (!isValid)
        {
            throw new ArgumentException("Invalid or expired reset token.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        
        // Explicitly clean up any remaining OTPs and the Reset Token after a successful reset
        await _otpService.DeleteResetTokenAsync(request.Email);
        await _otpService.DeleteOtpAsync(request.Email);
    }
}

