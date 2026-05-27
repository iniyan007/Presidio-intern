using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Packages;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Business.Services;

public class PackageService : IPackageService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IPackagerRepository _packagerRepository;

    public PackageService(IPackageRepository packageRepository, IPackagerRepository packagerRepository)
    {
        _packageRepository = packageRepository;
        _packagerRepository = packagerRepository;
    }

    public async Task<Guid> CreatePackageAsync(Guid userId, CreatePackageRequest request, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null || packager.ApprovedAt == null || packager.DeactivatedAt != null)
        {
            throw new UnauthorizedAccessException("Only approved packagers can create packages.");
        }

        var package = new Package
        {
            PackagerId = packager.Id,
            Title = request.Title,
            Description = request.Description,
            Destination = request.Destination,
            Country = request.Country,
            City = request.City,
            DurationDays = request.DurationDays,
            DurationNights = request.DurationNights,
            MaxCapacity = request.MaxCapacity,
            MinAge = request.MinAge,
            CancellationPolicy = request.CancellationPolicy,
            IsFeatured = false,
            CurrentBookings = 0,
            AvgRating = 0,
            TotalReviews = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TransientType = request.Type,
            TransientStatus = request.Status,
            PackageHighlights = request.Highlights?.Select(h => new PackageHighlight
            {
                HighlightText = h.HighlightText,
                DisplayOrder = h.DisplayOrder
            }).ToList() ?? new(),
            PackageInclusions = request.Inclusions?.Select(i => new PackageInclusion
            {
                Description = i.Description,
                DisplayOrder = i.DisplayOrder,
                TransientInclusionType = i.InclusionType
            }).ToList() ?? new(),
            PackageMedia = request.Media?.Select(m => new PackageMedium
            {
                FilePath = m.FilePath,
                FileName = m.FileName,
                Caption = m.Caption,
                DisplayOrder = m.DisplayOrder,
                IsPrimary = m.IsPrimary,
                TransientCategory = m.Category,
                UploadedAt = DateTime.UtcNow
            }).ToList() ?? new(),
            PackageSeasonalPricings = request.SeasonalPricing?.Select(p => new PackageSeasonalPricing
            {
                SeasonName = p.SeasonName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePrice = p.BasePrice,
                ChildPrice = p.ChildPrice,
                DiscountPercent = p.DiscountPercent,
                AvailableSlots = p.AvailableSlots,
                IsActive = p.IsActive
            }).ToList() ?? new(),
            ItineraryDays = request.Itinerary?.Select(day => new ItineraryDay
            {
                DayNumber = day.DayNumber,
                Title = day.Title,
                Description = day.Description,
                Location = day.Location,
                CreatedAt = DateTime.UtcNow,
                ItineraryActivities = day.Activities?.Select(a => new ItineraryActivity
                {
                    SequenceOrder = a.SequenceOrder,
                    ActivityTitle = a.ActivityTitle,
                    Description = a.Description,
                    ActivityType = a.ActivityType,
                    Location = a.Location,
                    DurationMinutes = a.DurationMinutes,
                    IsOptional = a.IsOptional,
                    ExtraCost = a.ExtraCost,
                    TransientDaySession = a.DaySession
                }).ToList() ?? new(),
                ItineraryDayMeals = day.Meals?.Select(m => new ItineraryDayMeal
                {
                    Venue = m.Venue,
                    Description = m.Description,
                    IsIncluded = m.IsIncluded,
                    TransientMealType = m.MealType
                }).ToList() ?? new(),
                PackageAccommodations = day.Accommodations?.Select(a => new PackageAccommodation
                {
                    HotelName = a.HotelName,
                    HotelAddress = a.HotelAddress,
                    StarRating = a.StarRating,
                    RoomType = a.RoomType,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    Amenities = a.Amenities,
                    Notes = a.Notes
                }).ToList() ?? new(),
                PackageTransports = day.Transports?.Select(t => new PackageTransport
                {
                    SegmentOrder = t.SegmentOrder,
                    VehicleDescription = t.VehicleDescription,
                    PickupPoint = t.PickupPoint,
                    DropPoint = t.DropPoint,
                    PickupTime = t.PickupTime,
                    DropTime = t.DropTime,
                    DistanceKm = t.DistanceKm,
                    Notes = t.Notes,
                    TransientTransportMode = t.TransportMode
                }).ToList() ?? new()
            }).ToList() ?? new()
        };

        var createdPackage = await _packageRepository.CreatePackageWithDetailsAsync(package, request.Type, request.Status, cancellationToken);
        return createdPackage.Id;
    }
}
