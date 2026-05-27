using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.DTOs.Reviews;

public record ReviewResponse(
    Guid Id,
    Guid BookingId,
    Guid UserId,
    string ReviewerName,
    short OverallRating,
    short? AccommodationRating,
    short? TransportRating,
    short? FoodRating,
    short? GuideRating,
    short? ValueRating,
    string? Comment,
    bool IsVerifiedTraveler,
    string? AdminNote,
    DateTime CreatedAt,
    List<ReviewMediaResponse> Media
);

public record ReviewMediaResponse(
    Guid Id,
    string FilePath
);
