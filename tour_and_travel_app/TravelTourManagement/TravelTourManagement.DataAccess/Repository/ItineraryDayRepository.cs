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
/// EF Core repository for the <see cref="ItineraryDay"/> entity.
/// </summary>
public class ItineraryDayRepository : GenericRepository<ItineraryDay, Guid>, IItineraryDayRepository
{
    public ItineraryDayRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ItineraryDay>> GetByPackageIdAsync(
        Guid packageId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(d => d.PackageId == packageId)
            .OrderBy(d => d.DayNumber)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<ItineraryDay?> GetWithFullDetailsAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(d => d.ItineraryActivities.OrderBy(a => a.SequenceOrder))
            .Include(d => d.ItineraryDayMeals)
            .Include(d => d.PackageAccommodations)
            .Include(d => d.PackageTransports.OrderBy(t => t.SegmentOrder))
            .FirstOrDefaultAsync(d => d.Id == itineraryDayId, cancellationToken);

    /// <inheritdoc />
    public async Task<int> GetMaxDayNumberAsync(
        Guid packageId,
        CancellationToken cancellationToken = default)
    {
        var max = await _dbSet
            .Where(d => d.PackageId == packageId)
            .MaxAsync(d => (int?)d.DayNumber, cancellationToken);

        return max ?? 0;
    }
}
