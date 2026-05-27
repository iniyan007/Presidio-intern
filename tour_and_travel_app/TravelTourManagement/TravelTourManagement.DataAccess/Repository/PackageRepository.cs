using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;

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
        => await _dbSet
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.AvgRating)
            .ToListAsync(cancellationToken);

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
        return await _dbSet
            .Where(p =>
                p.Destination.ToLower().Contains(lower) ||
                p.Country.ToLower().Contains(lower)     ||
                (p.City != null && p.City.ToLower().Contains(lower)) ||
                p.Title.ToLower().Contains(lower))
            .OrderByDescending(p => p.AvgRating)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Package>> GetByDestinationAsync(
        string destination,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.Destination.ToLower() == destination.ToLower())
            .OrderByDescending(p => p.AvgRating)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Package?> GetWithFullDetailsAsync(
        Guid packageId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.ItineraryDays)
            .Include(p => p.PackageHighlights)
            .Include(p => p.PackageInclusions)
            .Include(p => p.PackageMedia)
            .Include(p => p.PackageSeasonalPricings)
            .Include(p => p.Packager)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == packageId, cancellationToken);

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

    public async Task AddPackageMediaAsync(PackageMedium media, CancellationToken cancellationToken = default)
    {
        await _context.Set<PackageMedium>().AddAsync(media, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
