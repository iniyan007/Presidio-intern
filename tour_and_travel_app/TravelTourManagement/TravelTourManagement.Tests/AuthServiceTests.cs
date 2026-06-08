using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using TravelTourManagement.DataAccess.DTOs.Users;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;
using TravelTourManagement.Business.Providers;

namespace TravelTourManagement.Tests;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepoMock;
    private Mock<IPackagerRepository> _packagerRepoMock;
    private Mock<IJwtProvider> _jwtProviderMock;
    private Mock<IEmailService> _emailServiceMock;
    private Mock<IOtpService> _otpServiceMock;
    private Mock<IConfiguration> _configMock;
    private Mock<IMapper> _mapperMock;
    
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _packagerRepoMock = new Mock<IPackagerRepository>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _emailServiceMock = new Mock<IEmailService>();
        _otpServiceMock = new Mock<IOtpService>();
        _configMock = new Mock<IConfiguration>();
        _mapperMock = new Mock<IMapper>();

        _configMock.Setup(c => c["AdminSettings:Email"]).Returns("admin@tour.com");

        _authService = new AuthService(
            _userRepoMock.Object,
            _packagerRepoMock.Object,
            _jwtProviderMock.Object,
            _emailServiceMock.Object,
            _otpServiceMock.Object,
            _configMock.Object,
            _mapperMock.Object
        );
    }

    [Test]
    public async Task RegisterAsync_EmailExists_ThrowsInvalidOperationException()
    {
        var request = new RegisterUserRequest { FullName = "Test", Email = "test@test.com", Password = "Password123!" };
        _userRepoMock.Setup(x => x.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Func<Task> act = async () => await _authService.RegisterAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Email is already registered.");
    }

    [Test]
    public async Task RegisterAsync_ValidRequest_CreatesUserAndSendsOTP()
    {
        var request = new RegisterUserRequest { FullName = "John Doe", Email = "john@test.com", Password = "Password123!" };
        _userRepoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepoMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync((User u, CancellationToken c) => u);
        _otpServiceMock.Setup(x => x.GenerateAndStoreOtpAsync(It.IsAny<string>())).ReturnsAsync("123456");
        _jwtProviderMock.Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("token");
        _jwtProviderMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

        var response = await _authService.RegisterAsync(request);

        response.Should().NotBeNull();
        response.Token.Should().Be("token");
        _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        var request = new LoginRequest { Email = "missing@test.com", Password = "Pass" };
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        Func<Task> act = async () => await _authService.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid credentials.");
    }

    [Test]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var request = new LoginRequest { Email = "user@test.com", Password = "WrongPass" };
        var user = new User { Email = request.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass") };
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        Func<Task> act = async () => await _authService.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid credentials.");
    }

    [Test]
    public async Task LoginAsync_DeactivatedAccount_ThrowsUnauthorizedAccessException()
    {
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass" };
        var user = new User { Email = request.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass"), IsActive = false };
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        Func<Task> act = async () => await _authService.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Account is deactivated.");
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass" };
        var user = new User { Id = Guid.NewGuid(), Email = request.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass"), IsActive = true };
        
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtProviderMock.Setup(x => x.GenerateToken(user.Id, user.Email, "Traveler", user.IsEmailVerified)).Returns("token");
        _jwtProviderMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

        var response = await _authService.LoginAsync(request);

        response.Should().NotBeNull();
        response.Token.Should().Be("token");
        _userRepoMock.Verify(x => x.UpdateLastLoginAsync(user.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepoMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SendVerificationOtpAsync_AlreadyVerified_ThrowsInvalidOperationException()
    {
        var user = new User { Email = "user@test.com", IsEmailVerified = true };
        _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        Func<Task> act = async () => await _authService.SendVerificationOtpAsync(user.Email);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Email is already verified.");
    }

    [Test]
    public async Task VerifyEmailWithOtpAsync_InvalidOtp_ThrowsArgumentException()
    {
        var user = new User { Email = "user@test.com", IsEmailVerified = false };
        _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpServiceMock.Setup(x => x.VerifyOtpAsync(user.Email, "000000")).ReturnsAsync(false);

        Func<Task> act = async () => await _authService.VerifyEmailWithOtpAsync(user.Email, "000000");

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid or expired OTP.");
    }

    [Test]
    public async Task ForgotPasswordAsync_UserNotFound_ReturnsSilentlyToPreventEnumeration()
    {
        _userRepoMock.Setup(x => x.GetByEmailAsync("missing@test.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        Func<Task> act = async () => await _authService.ForgotPasswordAsync("missing@test.com");

        await act.Should().NotThrowAsync();
        _otpServiceMock.Verify(x => x.GenerateAndStoreOtpAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ResetPasswordAsync_ValidResetToken_UpdatesPassword()
    {
        var request = new ResetPasswordRequest { Email = "user@test.com", ResetToken = "resetToken", NewPassword = "NewPass123!" };
        var user = new User { Email = request.Email, PasswordHash = "oldHash" };
        
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpServiceMock.Setup(x => x.VerifyResetTokenAsync(request.Email, request.ResetToken)).ReturnsAsync(true);

        await _authService.ResetPasswordAsync(request);

        _userRepoMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.PasswordHash != "oldHash"), It.IsAny<CancellationToken>()), Times.Once);
        _otpServiceMock.Verify(x => x.DeleteResetTokenAsync(request.Email), Times.Once);
    }
}
