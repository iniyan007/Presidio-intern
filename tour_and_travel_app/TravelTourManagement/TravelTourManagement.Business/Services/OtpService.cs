using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using TravelTourManagement.Business.Interface;

namespace TravelTourManagement.Business.Services;

public class OtpService : IOtpService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan OtpExpiration = TimeSpan.FromMinutes(10);

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

    private static string GetCacheKey(string email) => $"OTP_{email.ToLowerInvariant()}";
}
