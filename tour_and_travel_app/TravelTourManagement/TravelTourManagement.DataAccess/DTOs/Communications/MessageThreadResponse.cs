using System;

namespace TravelTourManagement.DataAccess.DTOs.Communications;

public record MessageThreadResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid PackagerId,
    string PackagerName,
    Guid? PackageId,
    string? PackageTitle,
    DateTime CreatedAt,
    DateTime? LastMessageAt,
    int UnreadCount
);
