using System;

namespace TravelTourManagement.DataAccess.DTOs.Notifications;

public record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    Guid? ReferenceId,
    bool IsRead,
    DateTime CreatedAt
);
