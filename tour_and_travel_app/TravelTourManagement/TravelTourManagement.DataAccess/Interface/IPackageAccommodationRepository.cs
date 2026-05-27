using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="PackageAccommodation"/> entity.
/// </summary>
public interface IPackageAccommodationRepository : IRepository<PackageAccommodation, Guid>
{
    /// <summary>Returns all accommodation records for the given itinerary day.</summary>
    Task<IReadOnlyList<PackageAccommodation>> GetByItineraryDayIdAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-deletes all accommodation records for the given itinerary day.
    /// Typically called before re-creating the day's accommodation plan.
    /// </summary>
    Task DeleteByItineraryDayIdAsync(Guid itineraryDayId, CancellationToken cancellationToken = default);
}
