using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using TravelTourManagement.Business.Services;

namespace TravelTourManagement.Tests;

[TestFixture]
public class OtpServiceTests
{
    private MemoryDistributedCache _cache;
    private OtpService _otpService;

    [SetUp]
    public void Setup()
    {
        var opts = Options.Create(new MemoryDistributedCacheOptions());
        _cache = new MemoryDistributedCache(opts);
        _otpService = new OtpService(_cache);
    }

    [Test]
    public async Task GenerateAndStoreOtpAsync_GeneratesAndStoresSixDigitOtp()
    {
        var email = "test@example.com";
        var otp = await _otpService.GenerateAndStoreOtpAsync(email);

        otp.Should().NotBeNullOrWhiteSpace();
        otp.Length.Should().Be(6);

        var cachedBytes = await _cache.GetAsync($"OTP_{email}");
        cachedBytes.Should().NotBeNull();
        Encoding.UTF8.GetString(cachedBytes).Should().Be(otp);
    }

    [Test]
    public async Task VerifyOtpAsync_ValidOtp_ReturnsTrueAndRemovesFromCache()
    {
        var email = "test@example.com";
        var otp = await _otpService.GenerateAndStoreOtpAsync(email);

        var isValid = await _otpService.VerifyOtpAsync(email, otp);

        isValid.Should().BeTrue();

        var cachedBytes = await _cache.GetAsync($"OTP_{email}");
        cachedBytes.Should().BeNull(); // Should be removed after successful verification
    }

    [Test]
    public async Task VerifyOtpAsync_InvalidOtp_ReturnsFalseAndLeavesInCache()
    {
        var email = "test@example.com";
        var validOtp = await _otpService.GenerateAndStoreOtpAsync(email);

        var isValid = await _otpService.VerifyOtpAsync(email, "000000"); // Invalid

        isValid.Should().BeFalse();

        var cachedBytes = await _cache.GetAsync($"OTP_{email}");
        cachedBytes.Should().NotBeNull(); // Should not be removed
    }

    [Test]
    public async Task VerifyOtpAsync_NoOtpInCache_ReturnsFalse()
    {
        var isValid = await _otpService.VerifyOtpAsync("no_otp@example.com", "123456");
        isValid.Should().BeFalse();
    }

    [Test]
    public async Task GenerateAndStoreResetTokenAsync_GeneratesAndStoresToken()
    {
        var email = "test@example.com";
        var token = await _otpService.GenerateAndStoreResetTokenAsync(email);

        token.Should().NotBeNullOrWhiteSpace();

        var cachedBytes = await _cache.GetAsync($"RESET_TOKEN_{email}");
        cachedBytes.Should().NotBeNull();
        Encoding.UTF8.GetString(cachedBytes).Should().Be(token);
    }

    [Test]
    public async Task VerifyResetTokenAsync_ValidToken_ReturnsTrueAndRemovesFromCache()
    {
        var email = "test@example.com";
        var token = await _otpService.GenerateAndStoreResetTokenAsync(email);

        var isValid = await _otpService.VerifyResetTokenAsync(email, token);

        isValid.Should().BeTrue();

        var cachedBytes = await _cache.GetAsync($"RESET_TOKEN_{email}");
        cachedBytes.Should().BeNull();
    }

    [Test]
    public async Task VerifyResetTokenAsync_InvalidToken_ReturnsFalse()
    {
        var email = "test@example.com";
        await _otpService.GenerateAndStoreResetTokenAsync(email);

        var isValid = await _otpService.VerifyResetTokenAsync(email, "invalid_token");

        isValid.Should().BeFalse();
    }

    [Test]
    public async Task DeleteOtpAsync_RemovesOtpFromCache()
    {
        var email = "test@example.com";
        await _otpService.GenerateAndStoreOtpAsync(email);

        await _otpService.DeleteOtpAsync(email);

        var cachedBytes = await _cache.GetAsync($"OTP_{email}");
        cachedBytes.Should().BeNull();
    }

    [Test]
    public async Task DeleteResetTokenAsync_RemovesTokenFromCache()
    {
        var email = "test@example.com";
        await _otpService.GenerateAndStoreResetTokenAsync(email);

        await _otpService.DeleteResetTokenAsync(email);

        var cachedBytes = await _cache.GetAsync($"RESET_TOKEN_{email}");
        cachedBytes.Should().BeNull();
    }
}
