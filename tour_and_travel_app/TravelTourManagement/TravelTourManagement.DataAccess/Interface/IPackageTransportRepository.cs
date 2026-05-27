using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="PackageTransport"/> entity.
/// Manages transport segments (bus, flight, car, etc.) attached to itinerary days.
/// </summary>
public interface IPackageTransportRepository : IRepository<PackageTransport, Guid>
{
    /// <summary>
    /// Returns all transport segments for the given itinerary day,
    /// ordered by <c>SegmentOrder</c> ascending.
    /// </summary>
    Task<IReadOnlyList<PackageTransport>> GetByItineraryDayIdAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-deletes all transport segments for the given itinerary day.
    /// Typically called before re-creating the day's transport plan.
    /// </summary>
    Task DeleteByItineraryDayIdAsync(Guid itineraryDayId, CancellationToken cancellationToken = default);
}
