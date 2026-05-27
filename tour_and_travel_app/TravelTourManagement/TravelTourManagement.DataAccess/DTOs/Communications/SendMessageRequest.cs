using System;

namespace TravelTourManagement.DataAccess.DTOs.Communications;

public record SendMessageRequest(
    Guid? ThreadId,
    Guid? ReceiverId, // For starting a new thread
    Guid? PackageId,  // For starting a new thread context
    string Body
);
