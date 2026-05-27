namespace TravelTourManagement.DataAccess.DTOs.Users;

public record RegisterUserRequest(
    string FullName,
    string Email,
    string Password,
    string? Phone
);
