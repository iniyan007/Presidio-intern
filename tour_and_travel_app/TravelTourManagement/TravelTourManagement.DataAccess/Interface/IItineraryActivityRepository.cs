using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="ItineraryActivity"/> entity.
/// </summary>
public interface IItineraryActivityRepository : IRepository<ItineraryActivity, Guid>
{
    /// <summary>
    /// Returns all activities for the given itinerary day,
    /// ordered by <c>SequenceOrder</c> ascending.
    /// </summary>
    Task<IReadOnlyList<ItineraryActivity>> GetByItineraryDayIdAsync(
        Guid itineraryDayId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders activities within a day by updating their <c>SequenceOrder</c> values.
    /// </summary>
    /// <param name="orderedActivityIds">Activity IDs in the desired order, starting from 1.</param>
    Task ReorderAsync(IEnumerable<Guid> orderedActivityIds, CancellationToken cancellationToken = default);
}
