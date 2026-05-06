using backend.DTOs;

namespace backend.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
        Task<(bool Success, string Message, object? Data)> LoginAsync(LoginRequest request);
        Task<(bool Success, string Message, object? User)> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    }
}
