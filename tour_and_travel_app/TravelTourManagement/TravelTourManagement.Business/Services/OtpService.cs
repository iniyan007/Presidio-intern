using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.Business.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _memoryCache;
    private static readonly TimeSpan OtpExpiration = TimeSpan.FromMinutes(10);

    public OtpService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<string> GenerateAndStoreOtpAsync(string email)
    {
        // Generate a 6-digit random number
        var otp = new Random().Next(100000, 999999).ToString();
        var cacheKey = GetCacheKey(email);

        _memoryCache.Set(cacheKey, otp, OtpExpiration);

        return Task.FromResult(otp);
    }

    public Task<bool> VerifyOtpAsync(string email, string otp)
    {
        var cacheKey = GetCacheKey(email);

        if (_memoryCache.TryGetValue(cacheKey, out string? cachedOtp))
        {
            if (cachedOtp == otp)
            {
                // OTP is valid, remove it to prevent reuse
                _memoryCache.Remove(cacheKey);
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    private static string GetCacheKey(string email) => $"OTP_{email.ToLowerInvariant()}";
}
