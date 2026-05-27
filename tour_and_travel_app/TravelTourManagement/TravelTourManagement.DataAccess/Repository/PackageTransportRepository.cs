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
/// EF Core repository for the <see cref="PackageTransport"/> entity.
/// </summary>
public class PackageTransportRepository : GenericRepository<PackageTransport, Guid>, IPackageTransportRepository
{
    public PackageTransportRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PackageTransport>> GetByItineraryDayIdAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(t => t.ItineraryDayId == itineraryDayId)
            .OrderBy(t => t.SegmentOrder)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task DeleteByItineraryDayIdAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default)
    {
        var transports = await _dbSet
            .Where(t => t.ItineraryDayId == itineraryDayId)
            .ToListAsync(cancellationToken);

        if (transports.Count > 0)
        {
            _dbSet.RemoveRange(transports);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
