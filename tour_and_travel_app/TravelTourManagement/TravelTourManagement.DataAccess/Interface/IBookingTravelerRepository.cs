using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="BookingTraveler"/> entity.
/// </summary>
public interface IBookingTravelerRepository : IRepository<BookingTraveler, Guid>
{
    /// <summary>Returns all travelers associated with the specified booking.</summary>
    Task<IReadOnlyList<BookingTraveler>> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the primary traveler for a given booking,
    /// or <c>null</c> if none is designated as primary.
    /// </summary>
    Task<BookingTraveler?> GetPrimaryTravelerAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
