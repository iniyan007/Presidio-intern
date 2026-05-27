using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="Review"/> entity.
/// </summary>
public interface IReviewRepository : IRepository<Review, Guid>
{
    /// <summary>Returns all published reviews for the specified package.</summary>
    Task<IReadOnlyList<Review>> GetByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>Returns all published reviews targeting the specified packager.</summary>
    Task<IReadOnlyList<Review>> GetByPackagerIdAsync(Guid packagerId, CancellationToken cancellationToken = default);

    /// <summary>Returns all reviews authored by the specified user.</summary>
    Task<IReadOnlyList<Review>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the review for a specific booking,
    /// or <c>null</c> if no review has been submitted yet.
    /// </summary>
    Task<Review?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if the user has already submitted a review for the given booking.
    /// Used for duplicate-review guard checks.
    /// </summary>
    Task<bool> HasUserReviewedBookingAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default);
}
