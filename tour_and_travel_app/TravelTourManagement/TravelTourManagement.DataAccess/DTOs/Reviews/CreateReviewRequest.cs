using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.DTOs.Reviews;

public record CreateReviewRequest(
    Guid BookingId,
    short OverallRating,
    short? AccommodationRating,
    short? TransportRating,
    short? FoodRating,
    short? GuideRating,
    short? ValueRating,
    string? Comment,
    List<string> MediaFilePaths
);
