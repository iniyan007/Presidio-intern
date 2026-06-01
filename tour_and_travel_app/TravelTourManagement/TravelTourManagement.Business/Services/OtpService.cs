using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.Business.Services;

public class OtpService : IOtpService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan OtpExpiration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan TokenExpiration = TimeSpan.FromMinutes(15);

    public OtpService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string> GenerateAndStoreOtpAsync(string email)
    {
        // Generate a 6-digit random number
        var otp = new Random().Next(100000, 999999).ToString();
        var cacheKey = GetCacheKey(email);

        await _cache.SetStringAsync(cacheKey, otp, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = OtpExpiration });

        return otp;
    }

    public async Task<bool> VerifyOtpAsync(string email, string otp)
    {
        var cacheKey = GetCacheKey(email);

        var cachedOtp = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedOtp))
        {
            if (cachedOtp == otp)
            {
                // OTP is valid, remove it to prevent reuse
                await _cache.RemoveAsync(cacheKey);
                return true;
            }
        }

        return false;
    }

    public async Task<string> GenerateAndStoreResetTokenAsync(string email)
    {
        var token = Guid.NewGuid().ToString("N");
        var cacheKey = $"RESET_TOKEN_{email.ToLowerInvariant()}";

        await _cache.SetStringAsync(cacheKey, token, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TokenExpiration });

        return token;
    }

    public async Task<bool> VerifyResetTokenAsync(string email, string token)
    {
        var cacheKey = $"RESET_TOKEN_{email.ToLowerInvariant()}";

        var cachedToken = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedToken) && cachedToken == token)
        {
            await _cache.RemoveAsync(cacheKey);
            return true;
        }

        return false;
    }

    public async Task DeleteOtpAsync(string email)
    {
        var cacheKey = GetCacheKey(email);
        await _cache.RemoveAsync(cacheKey);
    }

    public async Task DeleteResetTokenAsync(string email)
    {
        var cacheKey = $"RESET_TOKEN_{email.ToLowerInvariant()}";
        await _cache.RemoveAsync(cacheKey);
    }

    private static string GetCacheKey(string email) => $"OTP_{email.ToLowerInvariant()}";
}
