using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="Booking"/> entity.
/// Extends the generic CRUD interface with booking-domain-specific queries.
/// </summary>
public interface IBookingRepository : IRepository<Booking, Guid>
{
    /// <summary>
    /// Retrieves a booking by its unique human-readable reference code,
    /// or <c>null</c> if no booking is found.
    /// </summary>
    Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default);

    /// <summary>Returns all bookings made by the specified user, newest first.</summary>
    Task<IReadOnlyList<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns all bookings associated with the specified package.</summary>
    Task<IReadOnlyList<Booking>> GetByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the booking together with its travelers, payments,
    /// travel documents, and the associated package.
    /// </summary>
    Task<Booking?> GetWithFullDetailsAsync(Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all bookings whose travel date falls within the specified range.
    /// </summary>
    Task<IReadOnlyList<Booking>> GetByTravelDateRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if a booking reference already exists in the system.
    /// Used during booking reference generation to guarantee uniqueness.
    /// </summary>
    Task<bool> ReferenceExistsAsync(string bookingReference, CancellationToken cancellationToken = default);
}
