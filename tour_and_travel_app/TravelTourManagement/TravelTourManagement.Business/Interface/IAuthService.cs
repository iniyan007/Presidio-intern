using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Users;

namespace TravelTourManagement.Business.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task SendVerificationOtpAsync(string email, CancellationToken cancellationToken = default);
    Task<AuthResponse> VerifyEmailWithOtpAsync(string email, string otp, CancellationToken cancellationToken = default);
}
