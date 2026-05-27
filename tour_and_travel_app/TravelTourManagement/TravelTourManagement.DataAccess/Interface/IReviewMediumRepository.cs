using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="ReviewMedium"/> entity.
/// Manages media files (photos/videos) attached to reviews.
/// </summary>
public interface IReviewMediumRepository : IRepository<ReviewMedium, Guid>
{
    /// <summary>Returns all media files attached to the specified review.</summary>
    Task<IReadOnlyList<ReviewMedium>> GetByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-deletes all media records associated with a review.
    /// Typically used when a review is removed.
    /// </summary>
    Task DeleteByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken = default);
}
