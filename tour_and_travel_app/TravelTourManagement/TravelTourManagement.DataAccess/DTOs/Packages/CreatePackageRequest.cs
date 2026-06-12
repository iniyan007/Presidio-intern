using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public record CreatePackageRequest(
    [Required] [MaxLength(200)] string Title,
    string? Description,
    [Required] [MaxLength(200)] string Destination,
    [Required] [MaxLength(100)] string Country,
    [MaxLength(100)] string? City,
    [Required] [Range(1, 365, ErrorMessage = "DurationDays must be between 1 and 365")] int DurationDays,
    [Range(0, 365, ErrorMessage = "DurationNights must be between 0 and 365")] int DurationNights,
    [Required] [Range(1, 1000, ErrorMessage = "MaxCapacity must be between 1 and 1000")] int MaxCapacity,
    [Range(0, 120, ErrorMessage = "MinAge must be between 0 and 120")] int? MinAge,
    string? CancellationPolicy,
    [Required] string Type, 
    [Required] string Status,
    
    List<CreatePackageHighlightRequest> Highlights,
    List<CreatePackageInclusionRequest> Inclusions,
    List<CreatePackageMediaRequest> Media,
    List<CreatePackagePricingRequest> SeasonalPricing,
    List<CreateItineraryDayRequest> Itinerary
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();
        bool isFlexibleDatePackage = string.Equals(Type, "Honeymoon", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(Type, "Private", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(Type, "Family", StringComparison.OrdinalIgnoreCase);

        if (SeasonalPricing != null)
        {
            foreach (var pricing in SeasonalPricing)
            {
                if (!isFlexibleDatePackage && (!pricing.StartDate.HasValue || !pricing.EndDate.HasValue))
                {
                    results.Add(new ValidationResult("StartDate and EndDate are required unless the package type is Honeymoon, Private, or Family.", new[] { nameof(SeasonalPricing) }));
                }
            }
        }

        if (!string.Equals(Country, "India", StringComparison.OrdinalIgnoreCase))
        {
            if (SeasonalPricing != null && SeasonalPricing.Any())
            {
                // Only validate if StartDate is provided (e.g. for non-Honeymoon, or Honeymoon with explicit dates)
                var validStartDates = SeasonalPricing.Where(p => p.StartDate.HasValue).Select(p => p.StartDate!.Value).ToList();
                if (validStartDates.Any())
                {
                    var earliestStartDate = validStartDates.Min();
                    var tenMonthsFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(10));
                    
                    if (earliestStartDate < tenMonthsFromNow)
                    {
                        results.Add(new ValidationResult("For international packages, the start date (earliest seasonal pricing) must be at least 10 months ahead of the current date.", new[] { nameof(SeasonalPricing) }));
                    }
                }
            }

            if (string.IsNullOrEmpty(CancellationPolicy) || !CancellationPolicy.Contains("3 months", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new ValidationResult("For international packages, the Cancellation Policy must state that cancellation is only allowed before 3 months from the date of departure.", new[] { nameof(CancellationPolicy) }));
            }
        }

        return results;
    }
}

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
    DateOnly? StartDate,
    DateOnly? EndDate,
    [Required] [Range(0.01, 10000000, ErrorMessage = "BasePrice must be strictly positive.")] decimal BasePrice,
    [Range(0, 10000000, ErrorMessage = "ChildPrice must be non-negative.")] decimal ChildPrice,
    [Range(0, 100, ErrorMessage = "DiscountPercent must be between 0 and 100")] decimal DiscountPercent,
    [Required] [Range(1, 10000, ErrorMessage = "AvailableSlots must be strictly positive.")] int AvailableSlots,
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
