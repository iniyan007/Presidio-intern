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
/// EF Core repository for the <see cref="ReviewMedium"/> entity.
/// Manages photos/videos attached to reviews.
/// </summary>
public class ReviewMediumRepository : GenericRepository<ReviewMedium, Guid>, IReviewMediumRepository
{
    public ReviewMediumRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReviewMedium>> GetByReviewIdAsync(
        Guid reviewId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(rm => rm.ReviewId == reviewId)
            .OrderBy(rm => rm.UploadedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task DeleteByReviewIdAsync(
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        var media = await _dbSet
            .Where(rm => rm.ReviewId == reviewId)
            .ToListAsync(cancellationToken);

        if (media.Count > 0)
        {
            _dbSet.RemoveRange(media);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
