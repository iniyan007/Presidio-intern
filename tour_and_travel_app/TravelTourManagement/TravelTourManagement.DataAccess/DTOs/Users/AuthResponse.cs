namespace TravelTourManagement.DataAccess.DTOs.Users;

public record AuthResponse(
    string Token,
    string RefreshToken,
    UserResponse User
);
