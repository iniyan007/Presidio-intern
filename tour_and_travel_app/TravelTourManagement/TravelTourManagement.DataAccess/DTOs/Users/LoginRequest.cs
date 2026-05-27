namespace TravelTourManagement.DataAccess.DTOs.Users;

public record LoginRequest(
    string Email,
    string Password
);
