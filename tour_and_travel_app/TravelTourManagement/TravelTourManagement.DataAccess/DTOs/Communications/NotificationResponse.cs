using System;

namespace TravelTourManagement.DataAccess.DTOs.Communications;

public record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    string? LinkUrl,
    bool IsRead,
    DateTime CreatedAt
);
