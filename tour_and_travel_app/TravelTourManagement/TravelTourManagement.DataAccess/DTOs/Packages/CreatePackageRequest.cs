using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public record CreatePackageRequest(
    [Required] [MaxLength(200)] string Title,
    string? Description,
    [Required] [MaxLength(200)] string Destination,
    [Required] [MaxLength(100)] string Country,
    [MaxLength(100)] string? City,
    [Required] int DurationDays,
    int DurationNights,
    [Required] int MaxCapacity,
    int? MinAge,
    string? CancellationPolicy,
    [Required] string Type, // Postgres Enum: package_type ('group', 'private', etc.)
    [Required] string Status, // Postgres Enum: package_status ('draft', 'published', etc.)
    
    List<CreatePackageHighlightRequest> Highlights,
    List<CreatePackageInclusionRequest> Inclusions,
    List<CreatePackageMediaRequest> Media,
    List<CreatePackagePricingRequest> SeasonalPricing,
    List<CreateItineraryDayRequest> Itinerary
);

public record CreatePackageHighlightRequest(
    [Required] string HighlightText,
    [Required] int DisplayOrder
);

public record CreatePackageInclusionRequest(
    [Required] string Description,
    [Required] int DisplayOrder,
    string InclusionType // Postgres Enum: inclusion_type ('included', 'excluded', 'optional')
);

public record CreatePackageMediaRequest(
    [Required] string FilePath,
    [Required] string FileName,
    string? Caption,
    int DisplayOrder,
    bool IsPrimary,
    [Required] string Category // Postgres Enum: media_category ('hotel', 'transport', 'activity', etc.)
);

public record CreatePackagePricingRequest(
    [Required] string SeasonName,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate,
    [Required] decimal BasePrice,
    decimal ChildPrice,
    decimal DiscountPercent,
    [Required] int AvailableSlots,
    bool IsActive
);

public record CreateItineraryDayRequest(
    [Required] int DayNumber,
    [Required] string Title,
    string? Description,
    string? Location,
    List<CreateItineraryActivityRequest> Activities,
    List<CreateItineraryMealRequest> Meals,
    List<CreatePackageAccommodationRequest> Accommodations,
    List<CreatePackageTransportRequest> Transports
);

public record CreateItineraryActivityRequest(
    [Required] int SequenceOrder,
    [Required] string ActivityTitle,
    string? Description,
    string? ActivityType,
    string? Location,
    int? DurationMinutes,
    bool IsOptional,
    decimal ExtraCost,
    string? DaySession // Postgres Enum: day_session ('morning', 'afternoon', etc.)
);

public record CreateItineraryMealRequest(
    string? Venue,
    string? Description,
    bool IsIncluded,
    string? MealType // Postgres Enum: meal_type ('breakfast', 'lunch', etc.)
);

public record CreatePackageAccommodationRequest(
    [Required] string HotelName,
    string? HotelAddress,
    short? StarRating,
    string? RoomType,
    string? CheckInTime,
    string? CheckOutTime,
    string? Amenities,
    string? Notes
);

public record CreatePackageTransportRequest(
    [Required] int SegmentOrder,
    string? VehicleDescription,
    [Required] string PickupPoint,
    [Required] string DropPoint,
    string? PickupTime,
    string? DropTime,
    decimal? DistanceKm,
    string? Notes,
    string? TransportMode // Postgres Enum: transport_mode ('bus', 'flight', etc.)
);
