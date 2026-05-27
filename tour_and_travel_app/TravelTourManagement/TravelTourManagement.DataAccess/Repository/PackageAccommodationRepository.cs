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
/// EF Core repository for the <see cref="PackageAccommodation"/> entity.
/// </summary>
public class PackageAccommodationRepository : GenericRepository<PackageAccommodation, Guid>, IPackageAccommodationRepository
{
    public PackageAccommodationRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PackageAccommodation>> GetByItineraryDayIdAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(a => a.ItineraryDayId == itineraryDayId)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task DeleteByItineraryDayIdAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default)
    {
        var accommodations = await _dbSet
            .Where(a => a.ItineraryDayId == itineraryDayId)
            .ToListAsync(cancellationToken);

        if (accommodations.Count > 0)
        {
            _dbSet.RemoveRange(accommodations);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
