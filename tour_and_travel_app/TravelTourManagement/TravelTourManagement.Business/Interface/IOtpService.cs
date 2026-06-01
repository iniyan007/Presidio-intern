using System.Threading.Tasks;

namespace TravelTourManagement.Business.Interface;

public interface IOtpService
{
    Task<string> GenerateAndStoreOtpAsync(string email);
    Task<bool> VerifyOtpAsync(string email, string otp);
    Task<string> GenerateAndStoreResetTokenAsync(string email);
    Task<bool> VerifyResetTokenAsync(string email, string token);
    Task DeleteOtpAsync(string email);
    Task DeleteResetTokenAsync(string email);
}
