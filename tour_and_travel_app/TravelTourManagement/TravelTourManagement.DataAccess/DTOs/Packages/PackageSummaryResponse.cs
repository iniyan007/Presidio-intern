using System;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public record PackageSummaryResponse(
    Guid Id,
    Guid PackagerId,
    string PackagerName,
    string Title,
    string PackageType,
    string Status,
    string Destination,
    string Country,
    int DurationDays,
    int DurationNights,
    decimal AvgRating,
    int TotalReviews,
    string? PrimaryImageUrl,
    decimal StartingPrice,
    int PendingSeats,
    DateOnly? EarliestDepartureDate,
    DateOnly? AvailableUntil
);
