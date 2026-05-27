namespace TravelTourManagement.DataAccess.DTOs.Users;

public record AuthResponse(
    string Token,
    UserResponse User
);
