using System;
using System.Collections.Generic;
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
            Type = Enum.Parse<TravelTourManagement.DataAccess.Enums.PackageType>(request.Type, true),
            Status = Enum.Parse<TravelTourManagement.DataAccess.Enums.PackageStatus>(request.Status, true),
            PackageHighlights = request.Highlights?.Select(h => new PackageHighlight
            {
                HighlightText = h.HighlightText,
                DisplayOrder = h.DisplayOrder
            }).ToList() ?? new(),
            PackageInclusions = request.Inclusions?.Select(i => new PackageInclusion
            {
                Description = i.Description,
                DisplayOrder = i.DisplayOrder,
                Type = Enum.Parse<TravelTourManagement.DataAccess.Enums.InclusionType>(i.InclusionType, true)
            }).ToList() ?? new(),
            PackageMedia = request.Media?.Select(m => new PackageMedium
            {
                FilePath = m.FilePath,
                FileName = m.FileName,
                Caption = m.Caption,
                DisplayOrder = m.DisplayOrder,
                IsPrimary = m.IsPrimary,
                Category = Enum.Parse<TravelTourManagement.DataAccess.Enums.MediaCategory>(m.Category, true),
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
                    DaySession = Enum.Parse<TravelTourManagement.DataAccess.Enums.DaySession>(a.DaySession, true)
                }).ToList() ?? new(),
                ItineraryDayMeals = day.Meals?.Select(m => new ItineraryDayMeal
                {
                    Venue = m.Venue,
                    Description = m.Description,
                    IsIncluded = m.IsIncluded,
                    MealType = Enum.Parse<TravelTourManagement.DataAccess.Enums.MealType>(m.MealType, true)
                }).ToList() ?? new(),
                PackageAccommodations = day.Accommodations?.Select(a => new PackageAccommodation
                {
                    HotelName = a.HotelName,
                    HotelAddress = a.HotelAddress,
                    StarRating = a.StarRating,
                    RoomType = a.RoomType,
                    CheckInTime = string.IsNullOrEmpty(a.CheckInTime) ? (TimeOnly?)null : TimeOnly.Parse(a.CheckInTime),
                    CheckOutTime = string.IsNullOrEmpty(a.CheckOutTime) ? (TimeOnly?)null : TimeOnly.Parse(a.CheckOutTime),
                    Amenities = a.Amenities,
                    Notes = a.Notes
                }).ToList() ?? new(),
                PackageTransports = day.Transports?.Select(t => new PackageTransport
                {
                    SegmentOrder = t.SegmentOrder,
                    VehicleDescription = t.VehicleDescription,
                    PickupPoint = t.PickupPoint,
                    DropPoint = t.DropPoint,
                    PickupTime = string.IsNullOrEmpty(t.PickupTime) ? (TimeOnly?)null : TimeOnly.Parse(t.PickupTime),
                    DropTime = string.IsNullOrEmpty(t.DropTime) ? (TimeOnly?)null : TimeOnly.Parse(t.DropTime),
                    DistanceKm = t.DistanceKm,
                    Notes = t.Notes,
                    TransportMode = Enum.Parse<TravelTourManagement.DataAccess.Enums.TransportMode>(t.TransportMode, true)
                }).ToList() ?? new()
            }).ToList() ?? new()
        };

        var createdPackage = await _packageRepository.CreatePackageWithDetailsAsync(package, cancellationToken);
        return createdPackage.Id;
    }

    public async Task DeletePackageAsync(Guid userId, Guid packageId, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null)
            throw new UnauthorizedAccessException("Only packagers can delete packages.");

        var package = await _packageRepository.GetByIdAsync(packageId, cancellationToken);
        if (package == null)
            throw new KeyNotFoundException("Package not found.");

        if (package.PackagerId != packager.Id)
            throw new UnauthorizedAccessException("You do not have permission to delete this package.");

        package.Status = TravelTourManagement.DataAccess.Enums.PackageStatus.Archived;
        package.UpdatedAt = DateTime.UtcNow;
        await _packageRepository.UpdateAsync(package, cancellationToken);
    }

    public async Task<string> UploadPackageMediaAsync(Guid userId, Guid packageId, System.IO.Stream fileStream, string fileName, string contentType, string category, bool isPrimary, int displayOrder, string? caption, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null)
            throw new UnauthorizedAccessException("Only packagers can modify packages.");

        var package = await _packageRepository.GetByIdAsync(packageId, cancellationToken);
        if (package == null)
            throw new KeyNotFoundException("Package not found.");

        if (package.PackagerId != packager.Id)
            throw new UnauthorizedAccessException("You do not have permission to modify this package.");

        var uploadDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "TravelTourManagement.DataAccess", "Uploads", "Packages");
        if (!System.IO.Directory.Exists(uploadDirectory))
        {
            System.IO.Directory.CreateDirectory(uploadDirectory);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = System.IO.Path.Combine(uploadDirectory, uniqueFileName);

        using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
        {
            await fileStream.CopyToAsync(stream, cancellationToken);
        }

        var media = new PackageMedium
        {
            PackageId = packageId,
            FileName = uniqueFileName,
            FilePath = filePath,
            MimeType = contentType,
            Category = Enum.Parse<TravelTourManagement.DataAccess.Enums.MediaCategory>(category, true),
            IsPrimary = isPrimary,
            DisplayOrder = displayOrder,
            Caption = caption,
            UploadedAt = DateTime.UtcNow
        };
        await _packageRepository.AddPackageMediaAsync(media, cancellationToken);

        return uniqueFileName;
    }

    public async Task<IEnumerable<PackageSummaryResponse>> GetAllPublishedPackagesAsync(CancellationToken cancellationToken = default)
    {
        var packages = await _packageRepository.GetAllPublishedWithFullDetailsAsync(cancellationToken);
        return packages.Select(MapToPackageSummaryResponse);
    }

    public async Task<PackageDetailResponse> GetPublishedPackageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetWithFullDetailsAsync(id, cancellationToken);
        if (package == null || package.Status != TravelTourManagement.DataAccess.Enums.PackageStatus.Published)
            throw new KeyNotFoundException("Published package not found.");

        return MapToPackageDetailResponse(package);
    }

    private static PackageSummaryResponse MapToPackageSummaryResponse(Package package)
    {
        var primaryImage = package.PackageMedia?.FirstOrDefault(m => m.IsPrimary)?.FilePath;
        var startingPrice = package.PackageSeasonalPricings?.Where(p => p.IsActive).OrderBy(p => p.BasePrice).FirstOrDefault()?.BasePrice ?? 0;
        var pendingSeats = package.MaxCapacity - package.CurrentBookings;

        return new PackageSummaryResponse(
            package.Id,
            package.PackagerId,
            package.Packager?.CompanyName ?? "Unknown",
            package.Title,
            package.Destination,
            package.Country,
            package.DurationDays,
            package.DurationNights,
            package.AvgRating,
            package.TotalReviews,
            primaryImage,
            startingPrice,
            pendingSeats
        );
    }

    private static PackageDetailResponse MapToPackageDetailResponse(Package package)
    {
        return new PackageDetailResponse(
            package.Id,
            package.PackagerId,
            package.Packager?.CompanyName ?? "Unknown",
            package.Title,
            package.Description,
            package.Destination,
            package.Country,
            package.City,
            package.DurationDays,
            package.DurationNights,
            package.MaxCapacity,
            package.CurrentBookings,
            package.MinAge,
            package.CancellationPolicy,
            package.IsFeatured,
            package.AvgRating,
            package.TotalReviews,
            package.PackageHighlights.OrderBy(h => h.DisplayOrder).Select(h => h.HighlightText).ToList(),
            package.PackageInclusions.Where(i => i.Type == TravelTourManagement.DataAccess.Enums.InclusionType.Included).Select(i => i.Description).ToList(),
            package.PackageInclusions.Where(i => i.Type == TravelTourManagement.DataAccess.Enums.InclusionType.Excluded).Select(i => i.Description).ToList(),
            package.PackageMedia.OrderBy(m => m.DisplayOrder).Select(m => new PackageMediaDto(m.Id, m.FilePath, m.Caption, m.IsPrimary, m.DisplayOrder)).ToList(),
            package.PackageSeasonalPricings.Select(p => new PackageSeasonalPricingDto(p.Id, p.SeasonName, p.StartDate, p.EndDate, p.BasePrice, p.ChildPrice, p.DiscountPercent, p.AvailableSlots, p.IsActive)).ToList(),
            package.ItineraryDays.OrderBy(d => d.DayNumber).Select(d => new ItineraryDayDto(
                d.Id,
                d.DayNumber,
                d.Title,
                d.Description,
                d.Location,
                d.ItineraryActivities.OrderBy(a => a.SequenceOrder).Select(a => new ItineraryActivityDto(
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
                d.ItineraryDayMeals.Select(m => new ItineraryMealDto(
                    m.Id,
                    m.Description,
                    m.Venue,
                    m.IsIncluded
                )).ToList(),
                d.PackageAccommodations.Select(a => new ItineraryAccommodationDto(
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
                d.PackageTransports.OrderBy(t => t.SegmentOrder).Select(t => new ItineraryTransportDto(
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
        );
    }
}
