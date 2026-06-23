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

    public PackageService(IPackageRepository packageRepository, IPackagerRepository packagerRepository, IBookingRepository bookingRepository, AutoMapper.IMapper mapper, IDistributedCache cache)
    {
        _packageRepository = packageRepository;
        _packagerRepository = packagerRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _cache = cache;
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
            var currentDirectory = Directory.GetCurrentDirectory(); 
            var solutionDirectory = Directory.GetParent(currentDirectory)?.FullName ?? currentDirectory;
            var uploadDirectory = Path.Combine(solutionDirectory, "TravelTourManagement.DataAccess", "Uploads", "Packages");
            
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            foreach (var mediaReq in request.Media)
            {
                var file = mediaFiles.FirstOrDefault(f => f.FileName == mediaReq.FileName);
                if (file != null && file.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadDirectory, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream, cancellationToken);
                    }

                    if (Enum.TryParse<TravelTourManagement.DataAccess.Enums.MediaCategory>(mediaReq.Category, true, out var category))
                    {
                        package.PackageMedia.Add(new PackageMedium
                        {
                            FileName = uniqueFileName,
                            FilePath = $"/api/Packages/media/{uniqueFileName}",
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

}