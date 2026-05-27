using System;

namespace TravelTourManagement.DataAccess.DTOs.Packagers;

public record PackagerResponse(
    Guid Id,
    Guid UserId,
    string CompanyName,
    string? BusinessLicenseNo,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    string? WebsiteUrl,
    string ApprovalStatus,
    string? Reason,
    decimal AvgRating,
    int TotalReviews,
    DateTime CreatedAt
);
