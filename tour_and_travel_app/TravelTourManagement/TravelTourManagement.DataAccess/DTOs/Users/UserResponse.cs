using System;

namespace TravelTourManagement.DataAccess.DTOs.Users;

public record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    string? ProfilePicture,
    bool IsActive,
    bool IsEmailVerified,
    bool IsPackager
);
