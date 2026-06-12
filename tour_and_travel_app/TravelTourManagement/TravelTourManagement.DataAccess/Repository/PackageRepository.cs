using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;

using TravelTourManagement.DataAccess.DTOs.Packages;
namespace TravelTourManagement.DataAccess.Repository;

/// <summary>
/// EF Core repository for the <see cref="Package"/> entity.
/// </summary>
public class PackageRepository : GenericRepository<Package, Guid>, IPackageRepository
{
    public PackageRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Package>> GetFeaturedPackagesAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet
            .Where(p => p.IsFeatured && p.PackageSeasonalPricings.Any(sp => sp.IsActive && sp.EndDate >= today))
            .OrderByDescending(p => p.AvgRating)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Package>> GetByPackagerIdAsync(
        Guid packagerId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.PackagerId == packagerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Package>> SearchAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        var lower = keyword.ToLower();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet
            .Where(p => p.PackageSeasonalPricings.Any(sp => sp.IsActive && sp.EndDate >= today) &&
                (p.Destination.ToLower().Contains(lower) ||
                p.Country.ToLower().Contains(lower)     ||
                (p.City != null && p.City.ToLower().Contains(lower)) ||
                p.Title.ToLower().Contains(lower)))
            .OrderByDescending(p => p.AvgRating)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Package>> GetByDestinationAsync(
        string destination,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet
            .Where(p => p.Destination.ToLower() == destination.ToLower() && p.PackageSeasonalPricings.Any(sp => sp.IsActive && sp.EndDate >= today))
            .OrderByDescending(p => p.AvgRating)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Package?> GetWithFullDetailsAsync(
        Guid packageId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.ItineraryDays)
                .ThenInclude(d => d.ItineraryActivities)
            .Include(p => p.ItineraryDays)
                .ThenInclude(d => d.ItineraryDayMeals)
            .Include(p => p.ItineraryDays)
                .ThenInclude(d => d.PackageAccommodations)
            .Include(p => p.ItineraryDays)
                .ThenInclude(d => d.PackageTransports)
            .Include(p => p.PackageHighlights)
            .Include(p => p.PackageInclusions)
            .Include(p => p.PackageMedia)
            .Include(p => p.PackageSeasonalPricings)
            .Include(p => p.Packager)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == packageId, cancellationToken);

    public async Task<IReadOnlyList<Package>> GetAllPublishedWithFullDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet
            .Where(p => p.Status == TravelTourManagement.DataAccess.Enums.PackageStatus.Published && p.PackageSeasonalPricings.Any(sp => sp.IsActive && sp.EndDate >= today))
            .Include(p => p.ItineraryDays)
                .ThenInclude(d => d.ItineraryActivities)
            .Include(p => p.ItineraryDays)
                .ThenInclude(d => d.ItineraryDayMeals)
            .Include(p => p.ItineraryDays)
                .ThenInclude(d => d.PackageAccommodations)
            .Include(p => p.ItineraryDays)
                .ThenInclude(d => d.PackageTransports)
            .Include(p => p.PackageHighlights)
            .Include(p => p.PackageInclusions)
            .Include(p => p.PackageMedia)
            .Include(p => p.PackageSeasonalPricings)
            .Include(p => p.Packager)
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Package>> GetAvailableByDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.PackageSeasonalPricings.Any(sp =>
                sp.IsActive &&
                sp.StartDate <= startDate &&
                sp.EndDate   >= endDate))
            .OrderByDescending(p => p.AvgRating)
            .ToListAsync(cancellationToken);
    /// <inheritdoc />
    public async Task<Package> CreatePackageWithDetailsAsync(
        Package package,
        CancellationToken cancellationToken = default)
    {
        // 1. Add the package and all its nested entities to the DbContext
        await _dbSet.AddAsync(package, cancellationToken);

        // 2. Save changes so that EF generates IDs for all entities
        await _context.SaveChangesAsync(cancellationToken);

        return package;
    }

    public async Task<List<string>> GetDistinctCountriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == TravelTourManagement.DataAccess.Enums.PackageStatus.Published)
            .Select(p => p.Country)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetDistinctDestinationsAsync(string? country = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Status == TravelTourManagement.DataAccess.Enums.PackageStatus.Published);
        
        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(p => p.Country.ToLower() == country.ToLower());
        }

        return await query
            .Select(p => p.Destination)
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync(cancellationToken);
    }


    
    public async Task<(IReadOnlyList<Package> Packages, int TotalCount)> SearchPackagesAsync(
        PackageSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = _dbSet.Where(p => p.Status == TravelTourManagement.DataAccess.Enums.PackageStatus.Published && p.PackageSeasonalPricings.Any(sp => sp.IsActive && sp.EndDate >= today));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var lowerTerm = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Title.ToLower().Contains(lowerTerm) ||
                p.Destination.ToLower().Contains(lowerTerm) ||
                p.Country.ToLower().Contains(lowerTerm) ||
                (p.City != null && p.City.ToLower().Contains(lowerTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.Destination))
        {
            var lowerDest = request.Destination.ToLower();
            query = query.Where(p => p.Destination.ToLower().Contains(lowerDest));
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            var lowerCountry = request.Country.ToLower();
            query = query.Where(p => p.Country.ToLower() == lowerCountry);
        }

        if (!string.IsNullOrWhiteSpace(request.PackageType))
        {
            if (Enum.TryParse<TravelTourManagement.DataAccess.Enums.PackageType>(request.PackageType, true, out var parsedType))
            {
                query = query.Where(p => p.Type == parsedType);
            }
        }

        if (request.MinDurationDays.HasValue)
            query = query.Where(p => p.DurationDays >= request.MinDurationDays.Value);

        if (request.MaxDurationDays.HasValue)
            query = query.Where(p => p.DurationDays <= request.MaxDurationDays.Value);

        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.PackageSeasonalPricings.Any(sp => sp.IsActive &&
                (!request.MinPrice.HasValue || sp.BasePrice >= request.MinPrice.Value) &&
                (!request.MaxPrice.HasValue || sp.BasePrice <= request.MaxPrice.Value)));
        }

        if (request.TravelStartDate.HasValue || request.TravelEndDate.HasValue)
        {
            query = query.Where(p => p.PackageSeasonalPricings.Any(sp => sp.IsActive && sp.AvailableSlots > 0 &&
                (!request.TravelStartDate.HasValue || sp.StartDate <= request.TravelStartDate.Value) &&
                (!request.TravelEndDate.HasValue || sp.EndDate >= request.TravelEndDate.Value)));
        }

        // Apply Sorting
        query = request.SortBy?.ToLower() switch
        {
            "priceasc" => query.OrderBy(p => p.PackageSeasonalPricings.Where(sp => sp.IsActive).Min(sp => sp.BasePrice)),
            "pricedesc" => query.OrderByDescending(p => p.PackageSeasonalPricings.Where(sp => sp.IsActive).Min(sp => sp.BasePrice)),
            "ratingdesc" => query.OrderByDescending(p => p.AvgRating),
            "durationasc" => query.OrderBy(p => p.DurationDays),
            "durationdesc" => query.OrderByDescending(p => p.DurationDays),
            _ => query.OrderByDescending(p => p.CreatedAt) // Default is Newest
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var packages = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(p => p.PackageMedia)
            .Include(p => p.PackageSeasonalPricings)
            .Include(p => p.Packager)
            .ToListAsync(cancellationToken);

        return (packages, totalCount);
    }

    public async Task AddPackageMediaAsync(PackageMedium media, CancellationToken cancellationToken = default)
    {
        await _context.Set<PackageMedium>().AddAsync(media, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
