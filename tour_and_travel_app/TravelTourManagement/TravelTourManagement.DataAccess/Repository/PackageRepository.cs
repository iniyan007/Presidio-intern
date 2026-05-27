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
        string packageType,
        string packageStatus,
        CancellationToken cancellationToken = default)
    {
        // 1. Add the package and all its nested entities to the DbContext
        await _dbSet.AddAsync(package, cancellationToken);

        // 2. Save changes so that EF generates IDs for all entities
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Update the postgres enums using Raw SQL since EF scaffolding missed them
        
        // Update Package type and status
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE packages SET package_type = {0}::package_type, status = {1}::package_status WHERE id = {2}",
            new object[] { packageType, packageStatus, package.Id },
            cancellationToken);

        // Update PackageInclusions
        foreach (var inc in package.PackageInclusions)
        {
            if (!string.IsNullOrEmpty(inc.TransientInclusionType))
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE package_inclusions SET type = {0}::inclusion_type WHERE id = {1}",
                    new object[] { inc.TransientInclusionType, inc.Id },
                    cancellationToken);
            }
        }

        // Update PackageMedia
        foreach (var media in package.PackageMedia)
        {
            if (!string.IsNullOrEmpty(media.TransientCategory))
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE package_media SET category = {0}::media_category WHERE id = {1}",
                    new object[] { media.TransientCategory, media.Id },
                    cancellationToken);
            }
        }

        // Iterate through Itinerary to update nested enum fields
        foreach (var day in package.ItineraryDays)
        {
            // Update Meals
            foreach (var meal in day.ItineraryDayMeals)
            {
                if (!string.IsNullOrEmpty(meal.TransientMealType))
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE itinerary_day_meals SET meal_type = {0}::meal_type WHERE id = {1}",
                        new object[] { meal.TransientMealType, meal.Id },
                        cancellationToken);
                }
            }

            // Update Transports
            foreach (var transport in day.PackageTransports)
            {
                if (!string.IsNullOrEmpty(transport.TransientTransportMode))
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE package_transport SET transport_mode = {0}::transport_mode WHERE id = {1}",
                        new object[] { transport.TransientTransportMode, transport.Id },
                        cancellationToken);
                }
            }

            // Update Activities
            foreach (var activity in day.ItineraryActivities)
            {
                if (!string.IsNullOrEmpty(activity.TransientDaySession))
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE itinerary_activities SET day_session = {0}::day_session WHERE id = {1}",
                        new object[] { activity.TransientDaySession, activity.Id },
                        cancellationToken);
                }
            }
        }

        return package;
    }
}
