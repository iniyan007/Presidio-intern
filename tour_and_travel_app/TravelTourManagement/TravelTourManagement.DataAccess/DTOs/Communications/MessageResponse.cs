using System;

namespace TravelTourManagement.DataAccess.DTOs.Communications;

public record MessageResponse(
    Guid Id,
    Guid ThreadId,
    Guid SenderId,
    string SenderName,
    string Body,
    bool IsRead,
    DateTime SentAt
);
