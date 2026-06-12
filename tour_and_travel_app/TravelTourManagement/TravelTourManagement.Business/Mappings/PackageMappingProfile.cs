using AutoMapper;
using System;
using System.Linq;
using TravelTourManagement.DataAccess.DTOs.Packages;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Mappings;

public class PackageMappingProfile : Profile
{
    public PackageMappingProfile()
    {
        // 1. DTO -> Entity (Create Package)
        CreateMap<CreatePackageRequest, Package>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PackagerId, opt => opt.Ignore())
            .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CurrentBookings, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.AvgRating, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Type) ? default : Enum.Parse<TravelTourManagement.DataAccess.Enums.PackageType>(src.Type, true)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Status) ? default : Enum.Parse<TravelTourManagement.DataAccess.Enums.PackageStatus>(src.Status, true)))
            .ForMember(dest => dest.PackageHighlights, opt => opt.MapFrom(src => src.Highlights))
            .ForMember(dest => dest.PackageInclusions, opt => opt.MapFrom(src => src.Inclusions))
            .ForMember(dest => dest.PackageSeasonalPricings, opt => opt.MapFrom(src => src.SeasonalPricing))
            .ForMember(dest => dest.ItineraryDays, opt => opt.MapFrom(src => src.Itinerary))
            .ForMember(dest => dest.PackageMedia, opt => opt.Ignore());

        CreateMap<CreatePackageHighlightRequest, PackageHighlight>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PackageId, opt => opt.Ignore())
            .ForMember(dest => dest.Package, opt => opt.Ignore());

        CreateMap<CreatePackageInclusionRequest, PackageInclusion>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PackageId, opt => opt.Ignore())
            .ForMember(dest => dest.Package, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.InclusionType) ? default : Enum.Parse<TravelTourManagement.DataAccess.Enums.InclusionType>(src.InclusionType, true)));

        CreateMap<CreatePackagePricingRequest, PackageSeasonalPricing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PackageId, opt => opt.Ignore())
            .ForMember(dest => dest.Package, opt => opt.Ignore())
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow)))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10))));

        CreateMap<CreateItineraryDayRequest, ItineraryDay>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PackageId, opt => opt.Ignore())
            .ForMember(dest => dest.Package, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.ItineraryActivities, opt => opt.MapFrom(src => src.Activities))
            .ForMember(dest => dest.ItineraryDayMeals, opt => opt.MapFrom(src => src.Meals))
            .ForMember(dest => dest.PackageAccommodations, opt => opt.MapFrom(src => src.Accommodations))
            .ForMember(dest => dest.PackageTransports, opt => opt.MapFrom(src => src.Transports));

        CreateMap<CreateItineraryActivityRequest, ItineraryActivity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ItineraryDayId, opt => opt.Ignore())
            .ForMember(dest => dest.ItineraryDay, opt => opt.Ignore())
            .ForMember(dest => dest.DaySession, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.DaySession) ? default : Enum.Parse<TravelTourManagement.DataAccess.Enums.DaySession>(src.DaySession, true)));

        CreateMap<CreateItineraryMealRequest, ItineraryDayMeal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ItineraryDayId, opt => opt.Ignore())
            .ForMember(dest => dest.ItineraryDay, opt => opt.Ignore())
            .ForMember(dest => dest.MealType, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.MealType) ? default : Enum.Parse<TravelTourManagement.DataAccess.Enums.MealType>(src.MealType, true)));

        CreateMap<CreatePackageAccommodationRequest, PackageAccommodation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ItineraryDayId, opt => opt.Ignore())
            .ForMember(dest => dest.ItineraryDay, opt => opt.Ignore())
            .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.CheckInTime) ? (TimeOnly?)null : TimeOnly.Parse(src.CheckInTime)))
            .ForMember(dest => dest.CheckOutTime, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.CheckOutTime) ? (TimeOnly?)null : TimeOnly.Parse(src.CheckOutTime)));

        CreateMap<CreatePackageTransportRequest, PackageTransport>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ItineraryDayId, opt => opt.Ignore())
            .ForMember(dest => dest.ItineraryDay, opt => opt.Ignore())
            .ForMember(dest => dest.PickupTime, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.PickupTime) ? (TimeOnly?)null : TimeOnly.Parse(src.PickupTime)))
            .ForMember(dest => dest.DropTime, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.DropTime) ? (TimeOnly?)null : TimeOnly.Parse(src.DropTime)))
            .ForMember(dest => dest.TransportMode, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.TransportMode) ? default : Enum.Parse<TravelTourManagement.DataAccess.Enums.TransportMode>(src.TransportMode, true)));

        // 2. Entity -> DTO (Package Summary)
        CreateMap<Package, PackageSummaryResponse>()
            .ConstructUsing((src, ctx) => new PackageSummaryResponse(
                src.Id,
                src.PackagerId,
                src.Packager != null ? src.Packager.CompanyName : "Unknown",
                src.Title,
                src.Type.ToString(),
                src.Destination,
                src.Country,
                src.DurationDays,
                src.DurationNights,
                src.AvgRating,
                src.TotalReviews,
                src.PackageMedia != null ? src.PackageMedia.FirstOrDefault(m => m.IsPrimary) != null ? src.PackageMedia.FirstOrDefault(m => m.IsPrimary)!.FilePath : null : null,
                src.PackageSeasonalPricings != null ? src.PackageSeasonalPricings.Where(p => p.IsActive).OrderBy(p => p.BasePrice).FirstOrDefault() != null ? src.PackageSeasonalPricings.Where(p => p.IsActive).OrderBy(p => p.BasePrice).FirstOrDefault()!.BasePrice : 0 : 0,
                src.MaxCapacity - src.CurrentBookings,
                src.PackageSeasonalPricings != null && src.PackageSeasonalPricings.Any(p => p.IsActive && p.StartDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                    ? src.PackageSeasonalPricings.Where(p => p.IsActive && p.StartDate >= DateOnly.FromDateTime(DateTime.UtcNow)).Min(p => p.StartDate)
                    : null,
                src.PackageSeasonalPricings != null && src.PackageSeasonalPricings.Any(p => p.IsActive)
                    ? src.PackageSeasonalPricings.Where(p => p.IsActive).Max(p => p.EndDate)
                    : null
            ));

        // 3. Entity -> DTO (Package Detail)
        CreateMap<Package, PackageDetailResponse>()
            .ForMember(dest => dest.ItineraryDays, opt => opt.Ignore())
            .ConstructUsing((src, ctx) => new PackageDetailResponse(
                src.Id,
                src.PackagerId,
                src.Packager != null ? src.Packager.CompanyName : "Unknown",
                src.Title,
                src.Type.ToString(),
                src.Description,
                src.Destination,
                src.Country,
                src.City,
                src.DurationDays,
                src.DurationNights,
                src.MaxCapacity,
                src.CurrentBookings,
                src.MinAge,
                src.CancellationPolicy,
                src.IsFeatured,
                src.AvgRating,
                src.TotalReviews,
                (src.PackageHighlights ?? Enumerable.Empty<PackageHighlight>()).OrderBy(h => h.DisplayOrder).Select(h => h.HighlightText).ToList(),
                (src.PackageInclusions ?? Enumerable.Empty<PackageInclusion>()).Where(i => i.Type == TravelTourManagement.DataAccess.Enums.InclusionType.Included).Select(i => i.Description).ToList(),
                (src.PackageInclusions ?? Enumerable.Empty<PackageInclusion>()).Where(i => i.Type == TravelTourManagement.DataAccess.Enums.InclusionType.Excluded).Select(i => i.Description).ToList(),
                (src.PackageMedia ?? Enumerable.Empty<PackageMedium>()).OrderBy(m => m.DisplayOrder).Select(m => new PackageMediaDto(m.Id, m.FilePath, m.Caption, m.IsPrimary, m.DisplayOrder)).ToList(),
                (src.PackageSeasonalPricings ?? Enumerable.Empty<PackageSeasonalPricing>()).Select(p => new PackageSeasonalPricingDto(p.Id, p.SeasonName, p.StartDate, p.EndDate, p.BasePrice, p.ChildPrice, p.DiscountPercent, p.AvailableSlots, p.IsActive)).ToList(),
                (src.ItineraryDays ?? Enumerable.Empty<ItineraryDay>()).OrderBy(d => d.DayNumber).Select(d => new ItineraryDayDto(
                    d.Id,
                    d.DayNumber,
                    d.Title,
                    d.Description,
                    d.Location,
                    (d.ItineraryActivities ?? Enumerable.Empty<ItineraryActivity>()).OrderBy(a => a.SequenceOrder).Select(a => new ItineraryActivityDto(
                        a.Id,
                        a.SequenceOrder,
                        a.ActivityTitle,
                        a.Description,
                        a.ActivityType,
                        a.Location,
                        a.DurationMinutes,
                        a.IsOptional,
                        a.ExtraCost
                    )).ToList(),
                    (d.ItineraryDayMeals ?? Enumerable.Empty<ItineraryDayMeal>()).Select(m => new ItineraryMealDto(
                        m.Id,
                        m.Description,
                        m.Venue,
                        m.IsIncluded
                    )).ToList(),
                    (d.PackageAccommodations ?? Enumerable.Empty<PackageAccommodation>()).Select(a => new ItineraryAccommodationDto(
                        a.Id,
                        a.HotelName,
                        a.HotelAddress,
                        a.RoomType,
                        a.StarRating,
                        a.CheckInTime.HasValue ? a.CheckInTime.Value.ToTimeSpan() : null,
                        a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToTimeSpan() : null,
                        a.Amenities,
                        a.Notes
                    )).ToList(),
                    (d.PackageTransports ?? Enumerable.Empty<PackageTransport>()).OrderBy(t => t.SegmentOrder).Select(t => new ItineraryTransportDto(
                        t.Id,
                        t.SegmentOrder,
                        t.VehicleDescription,
                        t.PickupPoint,
                        t.DropPoint,
                        t.PickupTime.HasValue ? t.PickupTime.Value.ToTimeSpan() : null,
                        t.DropTime.HasValue ? t.DropTime.Value.ToTimeSpan() : null,
                        t.DistanceKm,
                        t.Notes
                    )).ToList()
                )).ToList()
            ));
    }
}
