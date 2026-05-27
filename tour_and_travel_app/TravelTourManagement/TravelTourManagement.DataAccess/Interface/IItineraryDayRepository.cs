using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="ItineraryDay"/> entity.
/// </summary>
public interface IItineraryDayRepository : IRepository<ItineraryDay, Guid>
{
    /// <summary>
    /// Returns all itinerary days for the given package,
    /// ordered by <c>DayNumber</c> ascending.
    /// </summary>
    Task<IReadOnlyList<ItineraryDay>> GetByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the itinerary day together with its activities, meals,
    /// accommodation, and transport segments.
    /// </summary>
    Task<ItineraryDay?> GetWithFullDetailsAsync(Guid itineraryDayId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the maximum day number currently used in the package itinerary.
    /// Returns <c>0</c> if no days exist yet.
    /// </summary>
    Task<int> GetMaxDayNumberAsync(Guid packageId, CancellationToken cancellationToken = default);
}
