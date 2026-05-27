using System.Threading.Tasks;

namespace TravelTourManagement.Business.Interface;

public interface IOtpService
{
    Task<string> GenerateAndStoreOtpAsync(string email);
    Task<bool> VerifyOtpAsync(string email, string otp);
}
