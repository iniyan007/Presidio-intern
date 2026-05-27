using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public record PackageRequest(
    string Title,
    string? Description,
    string Destination,
    string Country,
    string? City,
    int DurationDays,
    int DurationNights,
    int MaxCapacity,
    int? MinAge,
    string? CancellationPolicy,
    
    // Nested creation records
    List<string> Highlights,
    List<string> Inclusions,
    List<string> Exclusions,
    List<ItineraryDayRequest> ItineraryDays
);

public record ItineraryDayRequest(
    int DayNumber,
    string Title,
    string? Description,
    string? Location,
    List<ItineraryActivityRequest> Activities,
    List<ItineraryMealRequest> Meals,
    List<ItineraryAccommodationRequest> Accommodations,
    List<ItineraryTransportRequest> Transports
);

public record ItineraryActivityRequest(
    int SequenceOrder,
    string ActivityTitle,
    string? Description,
    string? ActivityType,
    string? Location,
    int? DurationMinutes,
    bool IsOptional,
    decimal ExtraCost
);

public record ItineraryMealRequest(
    string? Description,
    string? Venue,
    bool IsIncluded
);

public record ItineraryAccommodationRequest(
    string HotelName,
    string? HotelAddress,
    string? RoomType,
    short? StarRating,
    TimeSpan? CheckInTime,
    TimeSpan? CheckOutTime,
    string? Amenities,
    string? Notes
);

public record ItineraryTransportRequest(
    int SegmentOrder,
    string? VehicleDescription,
    string? PickupPoint,
    string? DropPoint,
    TimeSpan? PickupTime,
    TimeSpan? DropTime,
    decimal? DistanceKm,
    string? Notes
);
