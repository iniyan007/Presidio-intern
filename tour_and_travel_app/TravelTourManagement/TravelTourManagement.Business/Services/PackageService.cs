using TravelTourManagement.DataAccess.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using TravelTourManagement.Business.Extensions;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Packages;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.Business.Services;

public class PackageService : IPackageService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IPackagerRepository _packagerRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly AutoMapper.IMapper _mapper;
    private readonly IDistributedCache _cache;
    private readonly IBlobStorageService _blobStorageService;

    public PackageService(IPackageRepository packageRepository, IPackagerRepository packagerRepository, IBookingRepository bookingRepository, AutoMapper.IMapper mapper, IDistributedCache cache, IBlobStorageService blobStorageService)
    {
        _packageRepository = packageRepository;
        _packagerRepository = packagerRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _cache = cache;
        _blobStorageService = blobStorageService;
    }

    public async Task<Guid> CreatePackageAsync(Guid userId, CreatePackageRequest request, List<IFormFile>? mediaFiles = null, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null || packager.ApprovedAt == null || packager.DeactivatedAt != null)
        {
            throw new UnauthorizedAccessException("Only approved packagers can create packages.");
        }

        var existingPackages = await _packageRepository.GetByPackagerIdAsync(packager.Id, cancellationToken);
        var normalize = (string? s) => s == null ? "" : System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ").Trim();
        var normalizedRequestTitle = normalize(request.Title);
        
        foreach (var p in existingPackages)
        {
            var normalizedExistingTitle = normalize(p.Title);
            if (string.Equals(normalizedExistingTitle, normalizedRequestTitle, StringComparison.OrdinalIgnoreCase))
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException($"A package with the title '{request.Title}' already exists in your account.");
            }
        }

        var package = _mapper.Map<Package>(request);
        package.PackagerId = packager.Id;
        package.PackageMedia = new List<PackageMedium>();

        if (request.Media != null && mediaFiles != null)
        {
            var availableFiles = mediaFiles.ToList();

            foreach (var mediaReq in request.Media)
            {
                var file = availableFiles.FirstOrDefault(f => f.FileName == mediaReq.FileName || Path.GetFileName(f.FileName) == mediaReq.FileName);
                if (file != null && file.Length > 0)
                {
                    availableFiles.Remove(file);
                    
                    if (file.OpenReadStream().CanSeek) file.OpenReadStream().Position = 0;

                    using var stream = file.OpenReadStream();
                    string fileUrl = await _blobStorageService.UploadFileAsync(stream, file.FileName, file.ContentType, "web-images", cancellationToken);

                    if (Enum.TryParse<TravelTourManagement.DataAccess.Enums.MediaCategory>(mediaReq.Category, true, out var category))
                    {
                        package.PackageMedia.Add(new PackageMedium
                        {
                            FileName = Path.GetFileName(new Uri(fileUrl).LocalPath),
                            FilePath = fileUrl,
                            Caption = mediaReq.Caption,
                            IsPrimary = mediaReq.IsPrimary,
                            Category = category,
                            DisplayOrder = mediaReq.DisplayOrder,
                            FileSizeBytes = file.Length,
                            MimeType = file.ContentType,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

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
        
        await _cache.RemoveAsync($"Package_{packageId}", cancellationToken);
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

        using var stream = fileStream;
        string fileUrl = await _blobStorageService.UploadFileAsync(stream, fileName, contentType, "web-images", cancellationToken);

        var media = new PackageMedium
        {
            PackageId = packageId,
            FileName = Path.GetFileName(new Uri(fileUrl).LocalPath),
            FilePath = fileUrl,
            MimeType = contentType,
            Category = Enum.Parse<TravelTourManagement.DataAccess.Enums.MediaCategory>(category, true),
            IsPrimary = isPrimary,
            DisplayOrder = displayOrder,
            Caption = caption,
            UploadedAt = DateTime.UtcNow
        };
        await _packageRepository.AddPackageMediaAsync(media, cancellationToken);

        return media.FilePath;
    }

    public async Task<IEnumerable<PackageSummaryResponse>> GetAllPublishedPackagesAsync(CancellationToken cancellationToken = default)
    {
        var packages = await _packageRepository.GetAllPublishedWithFullDetailsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<PackageSummaryResponse>>(packages);
    }

    public async Task<PackageDetailResponse> GetPublishedPackageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"Package_{id}";
        var cachedPackage = await _cache.GetRecordAsync<PackageDetailResponse>(cacheKey, cancellationToken);
        if (cachedPackage != null)
        {
            return cachedPackage;
        }

        var package = await _packageRepository.GetWithFullDetailsAsync(id, cancellationToken);
        if (package == null || package.Status != TravelTourManagement.DataAccess.Enums.PackageStatus.Published)
            throw new KeyNotFoundException("Published package not found.");

        var response = _mapper.Map<PackageDetailResponse>(package);

        await _cache.SetRecordAsync(cacheKey, response, TimeSpan.FromMinutes(2), null, cancellationToken);
        
        return response;
    }

    public async Task<PackageRevenueResponse> GetPackageRevenueAsync(Guid userId, string role, Guid packageId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(packageId, cancellationToken);
        if (package == null)
            throw new KeyNotFoundException("Package not found.");

        if (role == "Packager")
        {
            var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
            if (packager == null || package.PackagerId != packager.Id)
            {
                throw new UnauthorizedAccessException("You do not have permission to view revenue for this package.");
            }
        }

        var allBookings = await _bookingRepository.GetByPackageIdAsync(packageId, cancellationToken);
        var bookings = allBookings
            .Where(b => b.Status == TravelTourManagement.DataAccess.Enums.BookingStatus.Confirmed || b.Status == TravelTourManagement.DataAccess.Enums.BookingStatus.Completed)
            .ToList();

        decimal revenue = 0;
        string revenueType = "";

        if (role == "Admin")
        {
            revenue = bookings.Sum(b => b.PlatformFeeAmount);
            revenueType = "Platform Fee";
        }
        else if (role == "Packager")
        {
            revenue = bookings.Sum(b => b.PackagerBaseAmount);
            revenueType = "Packager Earnings";
        }

        return new PackageRevenueResponse
        {
            PackageId = packageId,
            PackageTitle = package.Title,
            Revenue = revenue,
            RevenueType = revenueType,
            TotalConfirmedBookings = bookings.Count
        };
    }

    public async Task UpdatePackageDetailsAsync(Guid userId, Guid packageId, UpdatePackageDetailsRequest request, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null)
            throw new UnauthorizedAccessException("Packager profile not found.");

        var package = await _packageRepository.GetByIdAsync(packageId, cancellationToken);
        if (package == null)
            throw new KeyNotFoundException("Package not found.");

        if (package.PackagerId != packager.Id)
            throw new UnauthorizedAccessException("You do not own this package.");

        // Check for duplicate title
        if (!string.Equals(package.Title, request.Title, StringComparison.OrdinalIgnoreCase))
        {
            var existingPackages = await _packageRepository.GetByPackagerIdAsync(packager.Id, cancellationToken);
            var normalize = (string? s) => s == null ? "" : System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ").Trim();
            var normalizedRequestTitle = normalize(request.Title);
            
            foreach (var p in existingPackages)
            {
                if (p.Id == package.Id) continue; // Skip the current package
                
                var normalizedExistingTitle = normalize(p.Title);
                if (string.Equals(normalizedExistingTitle, normalizedRequestTitle, StringComparison.OrdinalIgnoreCase))
                {
                    throw new System.ComponentModel.DataAnnotations.ValidationException($"A package with the title '{request.Title}' already exists in your account.");
                }
            }
        }

        package.Title = request.Title;
        package.Description = request.Description;
        package.Destination = request.Destination;
        package.Country = request.Country;
        package.City = request.City;
        package.DurationDays = request.DurationDays;
        package.DurationNights = request.DurationNights;
        package.MaxCapacity = request.MaxCapacity;
        package.MinAge = request.MinAge;
        package.CancellationPolicy = request.CancellationPolicy;
        package.UpdatedAt = DateTime.UtcNow;

        await _packageRepository.UpdateAsync(package, cancellationToken);
        
        await _cache.RemoveAsync($"Package_{packageId}", cancellationToken);
    }

    public async Task RepublishPackageAsync(Guid userId, Guid packageId, RepublishPackageRequest request, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null)
            throw new UnauthorizedAccessException("Packager profile not found.");

        var package = await _packageRepository.GetByIdAsync(packageId, cancellationToken);
        if (package == null)
            throw new KeyNotFoundException("Package not found.");

        if (package.PackagerId != packager.Id)
            throw new UnauthorizedAccessException("You do not own this package.");

        // Clear existing pricing logic or add new? The user requested to ADD new dates to the EXISTING package to preserve reviews.
        // We will map the new SeasonalPricing items and add them.
        foreach (var pricingReq in request.SeasonalPricing)
        {
            var pricing = new PackageSeasonalPricing
            {
                Id = Guid.NewGuid(),
                PackageId = package.Id,
                SeasonName = pricingReq.SeasonName,
                StartDate = pricingReq.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = pricingReq.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                BasePrice = pricingReq.BasePrice,
                ChildPrice = pricingReq.ChildPrice,
                DiscountPercent = pricingReq.DiscountPercent,
                AvailableSlots = pricingReq.AvailableSlots,
                IsActive = pricingReq.IsActive
            };
            package.PackageSeasonalPricings.Add(pricing);
        }

        // If package was archived or completed, set it back to Published or PendingReview depending on business rules.
        // We'll set it back to Published to make it immediately active (since it was previously approved).
        if (package.Status == PackageStatus.Archived || package.Status == PackageStatus.Completed)
        {
            package.Status = PackageStatus.Published;
        }

        package.UpdatedAt = DateTime.UtcNow;
        await _packageRepository.UpdateAsync(package, cancellationToken);
    }

    public async Task<PagedResponse<PackageSummaryResponse>> SearchPackagesAsync(PackageSearchRequest request, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"SearchPackages_{request.PageNumber}_{request.PageSize}_{request.SearchTerm}_{request.Destination}_{request.Country}_{request.PackageType}_{request.PackagerName}_{request.MinPrice}_{request.MaxPrice}_{request.MinDurationDays}_{request.MaxDurationDays}_{request.SortBy}";
        
        var cachedResponse = await _cache.GetRecordAsync<PagedResponse<PackageSummaryResponse>>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        var (packages, totalCount) = await _packageRepository.SearchPackagesAsync(request, cancellationToken);
        var summaryResponses = _mapper.Map<List<PackageSummaryResponse>>(packages);
        
        var response = new PagedResponse<PackageSummaryResponse>(summaryResponses, totalCount, request.PageNumber, request.PageSize);

        await _cache.SetRecordAsync(cacheKey, response, TimeSpan.FromMinutes(2), null, cancellationToken);

        return response;
    }

    public async Task<IEnumerable<PackageSummaryResponse>> GetMyPackagesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null)
            throw new UnauthorizedAccessException("Packager profile not found.");

        var packages = await _packageRepository.GetByPackagerIdAsync(packager.Id, cancellationToken);
        
        // We might want to use a custom mapping for 'MyPackages' to include the Status,
        // but PackageSummaryResponse already maps fields. Let's see if Status is in Summary.
        // It's probably mapped.
        return _mapper.Map<IEnumerable<PackageSummaryResponse>>(packages);
    }

    public async Task<PackageDetailResponse> GetMyPackageByIdAsync(Guid userId, Guid packageId, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null)
            throw new UnauthorizedAccessException("Packager profile not found.");

        var package = await _packageRepository.GetWithFullDetailsAsync(packageId, cancellationToken);
        if (package == null)
            throw new KeyNotFoundException("Package not found.");

        if (package.PackagerId != packager.Id)
            throw new UnauthorizedAccessException("You do not own this package.");

        return _mapper.Map<PackageDetailResponse>(package);
    }

    public async Task UpdateFullPackageAsync(Guid userId, Guid packageId, CreatePackageRequest request, List<IFormFile>? mediaFiles = null, CancellationToken cancellationToken = default)
    {
        var packager = await _packagerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (packager == null)
            throw new UnauthorizedAccessException("Packager profile not found.");

        var package = await _packageRepository.GetWithFullDetailsAsync(packageId, cancellationToken);
        if (package == null)
            throw new KeyNotFoundException("Package not found.");

        if (package.PackagerId != packager.Id)
            throw new UnauthorizedAccessException("You do not own this package.");

        if (package.Status != PackageStatus.Draft && package.Status != PackageStatus.PendingReview && package.Status != PackageStatus.Archived && package.Status != PackageStatus.Published)
        {
            throw new InvalidOperationException("Only draft, pending, archived, or active packages can be fully edited.");
        }

        bool isPublished = package.Status == PackageStatus.Published;
        var bookings = await _bookingRepository.GetByPackageIdAsync(packageId, cancellationToken);
        bool hasBookings = bookings.Any();
        bool preserveCoreData = isPublished || hasBookings;

        // Validate title uniqueness
        var existingPackages = await _packageRepository.GetByPackagerIdAsync(packager.Id, cancellationToken);
        var normalize = (string? s) => s == null ? "" : System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ").Trim();
        var normalizedRequestTitle = normalize(request.Title);
        foreach (var p in existingPackages)
        {
            if (p.Id == packageId) continue;
            var normalizedExistingTitle = normalize(p.Title);
            if (string.Equals(normalizedExistingTitle, normalizedRequestTitle, StringComparison.OrdinalIgnoreCase))
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException($"A package with the title '{request.Title}' already exists in your account.");
            }
        }
        var oldMedia = package.PackageMedia.ToList();
        
        await _packageRepository.DeletePackageCollectionsAsync(package, preserveCoreData, cancellationToken);
        
        package.PackageHighlights.Clear();
        package.PackageInclusions.Clear();
        package.PackageMedia.Clear();
        
        if (!preserveCoreData)
        {
            package.PackageSeasonalPricings.Clear();
            package.ItineraryDays.Clear();
        }

        if (!preserveCoreData)
        {
            package.Title = request.Title;
            package.Description = request.Description;
            package.Destination = request.Destination;
            package.Country = request.Country;
            package.City = request.City;
            package.DurationDays = request.DurationDays;
            package.DurationNights = request.DurationNights;
            package.MaxCapacity = request.MaxCapacity;
            package.MinAge = request.MinAge;
            package.CancellationPolicy = request.CancellationPolicy;
            if (Enum.TryParse<PackageType>(request.Type, true, out var pType)) package.Type = pType;
        }
        
        if (Enum.TryParse<PackageStatus>(request.Status, true, out var pStatus)) package.Status = pStatus;

        package.UpdatedAt = DateTime.UtcNow;

        // Add new nested collections from request
        foreach (var h in request.Highlights ?? Enumerable.Empty<CreatePackageHighlightRequest>())
            package.PackageHighlights.Add(new PackageHighlight { PackageId = package.Id, HighlightText = h.HighlightText, DisplayOrder = h.DisplayOrder });

        foreach (var inc in request.Inclusions ?? Enumerable.Empty<CreatePackageInclusionRequest>())
            package.PackageInclusions.Add(new PackageInclusion { PackageId = package.Id, Description = inc.Description, DisplayOrder = inc.DisplayOrder, Type = string.IsNullOrEmpty(inc.InclusionType) ? default : Enum.Parse<InclusionType>(inc.InclusionType, true) });

        if (preserveCoreData)
        {
            var requestIds = request.SeasonalPricing?.Where(sp => sp.Id.HasValue).Select(sp => sp.Id!.Value).ToHashSet() ?? new HashSet<Guid>();
            
            // Note: We DO NOT remove existing pricing if preserveCoreData is true, because they might be referenced by bookings.
            // We just update matching ones, and add new ones.
            foreach (var sp in request.SeasonalPricing ?? Enumerable.Empty<CreatePackagePricingRequest>())
            {
                if (sp.Id.HasValue && sp.Id.Value != Guid.Empty)
                {
                    var existing = package.PackageSeasonalPricings.FirstOrDefault(p => p.Id == sp.Id.Value);
                    if (existing != null)
                    {
                        existing.SeasonName = sp.SeasonName;
                        existing.StartDate = sp.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
                        existing.EndDate = sp.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10));
                        existing.BasePrice = sp.BasePrice;
                        existing.ChildPrice = sp.ChildPrice;
                        existing.DiscountPercent = sp.DiscountPercent;
                        existing.AvailableSlots = sp.AvailableSlots;
                        existing.IsActive = sp.IsActive;
                    }
                }
                else
                {
                    package.PackageSeasonalPricings.Add(new PackageSeasonalPricing { PackageId = package.Id, SeasonName = sp.SeasonName, StartDate = sp.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow), EndDate = sp.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10)), BasePrice = sp.BasePrice, ChildPrice = sp.ChildPrice, DiscountPercent = sp.DiscountPercent, AvailableSlots = sp.AvailableSlots, IsActive = sp.IsActive });
                }
            }
        }
        else
        {
            foreach (var sp in request.SeasonalPricing ?? Enumerable.Empty<CreatePackagePricingRequest>())
                package.PackageSeasonalPricings.Add(new PackageSeasonalPricing { PackageId = package.Id, SeasonName = sp.SeasonName, StartDate = sp.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow), EndDate = sp.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10)), BasePrice = sp.BasePrice, ChildPrice = sp.ChildPrice, DiscountPercent = sp.DiscountPercent, AvailableSlots = sp.AvailableSlots, IsActive = sp.IsActive });
        }

        if (!preserveCoreData)
        {
            foreach (var day in request.Itinerary ?? Enumerable.Empty<CreateItineraryDayRequest>())
            {
                var newDay = new ItineraryDay { PackageId = package.Id, DayNumber = day.DayNumber, Title = day.Title, Description = day.Description, Location = day.Location };
                foreach (var a in day.Activities ?? Enumerable.Empty<CreateItineraryActivityRequest>())
                    newDay.ItineraryActivities.Add(new ItineraryActivity { ItineraryDayId = newDay.Id, SequenceOrder = a.SequenceOrder, ActivityTitle = a.ActivityTitle, Description = a.Description, ActivityType = a.ActivityType, Location = a.Location, DurationMinutes = a.DurationMinutes, IsOptional = a.IsOptional, ExtraCost = a.ExtraCost, DaySession = string.IsNullOrEmpty(a.DaySession) ? default : Enum.Parse<DaySession>(a.DaySession, true) });
                foreach (var m in day.Meals ?? Enumerable.Empty<CreateItineraryMealRequest>())
                    newDay.ItineraryDayMeals.Add(new ItineraryDayMeal { ItineraryDayId = newDay.Id, Venue = m.Venue, Description = m.Description, IsIncluded = m.IsIncluded, MealType = string.IsNullOrEmpty(m.MealType) ? default : Enum.Parse<MealType>(m.MealType, true) });
                foreach (var acc in day.Accommodations ?? Enumerable.Empty<CreatePackageAccommodationRequest>())
                    newDay.PackageAccommodations.Add(new PackageAccommodation { ItineraryDayId = newDay.Id, HotelName = acc.HotelName, HotelAddress = acc.HotelAddress, StarRating = acc.StarRating, RoomType = acc.RoomType, CheckInTime = string.IsNullOrEmpty(acc.CheckInTime) ? null : TimeOnly.Parse(acc.CheckInTime), CheckOutTime = string.IsNullOrEmpty(acc.CheckOutTime) ? null : TimeOnly.Parse(acc.CheckOutTime), Amenities = acc.Amenities, Notes = acc.Notes });
                foreach (var tr in day.Transports ?? Enumerable.Empty<CreatePackageTransportRequest>())
                    newDay.PackageTransports.Add(new PackageTransport { ItineraryDayId = newDay.Id, SegmentOrder = tr.SegmentOrder, VehicleDescription = tr.VehicleDescription, PickupPoint = tr.PickupPoint, DropPoint = tr.DropPoint, PickupTime = string.IsNullOrEmpty(tr.PickupTime) ? null : TimeOnly.Parse(tr.PickupTime), DropTime = string.IsNullOrEmpty(tr.DropTime) ? null : TimeOnly.Parse(tr.DropTime), DistanceKm = tr.DistanceKm, Notes = tr.Notes, TransportMode = string.IsNullOrEmpty(tr.TransportMode) ? default : Enum.Parse<TransportMode>(tr.TransportMode, true) });
                
                package.ItineraryDays.Add(newDay);
            }
        }

        // Handle Media: retain existing if their file is not in mediaFiles, but replace order/caption etc.
        // For simplicity, we can remove old media and upload new ones, OR update existing based on FileName match.
        
        var availableFiles = mediaFiles?.ToList() ?? new List<IFormFile>();

        foreach (var mediaReq in request.Media ?? Enumerable.Empty<CreatePackageMediaRequest>())
        {
            var existingMedia = oldMedia.FirstOrDefault(m => m.FileName == mediaReq.FileName || m.FileName.EndsWith("_" + mediaReq.FileName));
            
            var file = availableFiles.FirstOrDefault(f => f.FileName == mediaReq.FileName || Path.GetFileName(f.FileName) == mediaReq.FileName);
            if (file != null && file.Length > 0)
            {
                availableFiles.Remove(file);
                if (file.OpenReadStream().CanSeek) file.OpenReadStream().Position = 0;
                
                using var stream = file.OpenReadStream();
                string fileUrl = await _blobStorageService.UploadFileAsync(stream, file.FileName, file.ContentType, "web-images", cancellationToken);
                
                package.PackageMedia.Add(new PackageMedium
                {
                    PackageId = package.Id,
                    FileName = Path.GetFileName(new Uri(fileUrl).LocalPath),
                    FilePath = fileUrl,
                    Caption = mediaReq.Caption,
                    IsPrimary = mediaReq.IsPrimary,
                    Category = Enum.Parse<MediaCategory>(mediaReq.Category, true),
                    DisplayOrder = mediaReq.DisplayOrder,
                    FileSizeBytes = file.Length,
                    MimeType = file.ContentType,
                    UploadedAt = DateTime.UtcNow
                });
            }
            else if (existingMedia != null)
            {
                // Preserve existing file but update metadata
                package.PackageMedia.Add(new PackageMedium
                {
                    PackageId = package.Id,
                    FileName = existingMedia.FileName,
                    FilePath = existingMedia.FilePath,
                    Caption = mediaReq.Caption,
                    IsPrimary = mediaReq.IsPrimary,
                    Category = Enum.Parse<MediaCategory>(mediaReq.Category, true),
                    DisplayOrder = mediaReq.DisplayOrder,
                    FileSizeBytes = existingMedia.FileSizeBytes,
                    MimeType = existingMedia.MimeType,
                    UploadedAt = existingMedia.UploadedAt
                });
            }
        }

        // Clean up orphaned files from Blob Storage
        var preservedFileNames = package.PackageMedia.Select(m => m.FileName).ToHashSet();
        foreach (var old in oldMedia)
        {
            if (!preservedFileNames.Contains(old.FileName))
            {
                await _blobStorageService.DeleteFileAsync(old.FilePath, "web-images", cancellationToken);
            }
        }

        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _cache.RemoveAsync($"Package_{packageId}", cancellationToken);
    }
}