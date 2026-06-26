using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public record PackageDetailResponse(
    Guid Id,
    Guid PackagerId,
    string PackagerName,
    string Title,
    string PackageType,
    string Status,
    string? Description,
    string Destination,
    string Country,
    string? City,
    int DurationDays,
    int DurationNights,
    int MaxCapacity,
    int CurrentBookings,
    int? MinAge,
    string? CancellationPolicy,
    bool IsFeatured,
    decimal AvgRating,
    int TotalReviews,
    decimal? AvgAccommodationRating,
    decimal? AvgTransportRating,
    decimal? AvgFoodRating,
    decimal? AvgGuideRating,
    decimal? AvgValueRating,

    List<string> Highlights,
    List<string> Inclusions,
    List<string> Exclusions,
    List<PackageMediaDto> Media,
    List<PackageSeasonalPricingDto> SeasonalPricings,
    List<ItineraryDayDto> ItineraryDays
);

public record PackageMediaDto(
    Guid Id,
    string FileName,
    string FilePath,
    string? MimeType,
    string Category,
    string? Caption,
    bool IsPrimary,
    int DisplayOrder
);

public record PackageSeasonalPricingDto(
    Guid Id,
    string SeasonName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal BasePrice,
    decimal? ChildPrice,
    decimal? DiscountPercent,
    int AvailableSlots,
    bool IsActive
);

public record ItineraryDayDto(
    Guid Id,
    int DayNumber,
    string Title,
    string? Description,
    string? Location,
    List<ItineraryActivityDto> Activities,
    List<ItineraryMealDto> Meals,
    List<ItineraryAccommodationDto> Accommodations,
    List<ItineraryTransportDto> Transports
);

public record ItineraryActivityDto(
    Guid Id,
    int SequenceOrder,
    string ActivityTitle,
    string? Description,
    string? ActivityType,
    string? Location,
    int? DurationMinutes,
    bool IsOptional,
    decimal ExtraCost,
    string? DaySession
);

public record ItineraryMealDto(
    Guid Id,
    string? Description,
    string? Venue,
    bool IsIncluded,
    string? MealType
);

public record ItineraryAccommodationDto(
    Guid Id,
    string HotelName,
    string? HotelAddress,
    string? RoomType,
    short? StarRating,
    TimeSpan? CheckInTime,
    TimeSpan? CheckOutTime,
    string? Amenities,
    string? Notes
);

public record ItineraryTransportDto(
    Guid Id,
    int SegmentOrder,
    string? VehicleDescription,
    string? PickupPoint,
    string? DropPoint,
    TimeSpan? PickupTime,
    TimeSpan? DropTime,
    decimal? DistanceKm,
    string? Notes,
    string? TransportMode
);
