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
/// EF Core repository for the <see cref="Review"/> entity.
/// </summary>
public class ReviewRepository : GenericRepository<Review, Guid>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Review>> GetByPackageIdAsync(
        Guid packageId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(r => r.User)
            .Include(r => r.ReviewMedia)
            .Where(r => r.PackageId == packageId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Review>> GetByPackagerIdAsync(
        Guid packagerId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(r => r.User)
            .Include(r => r.ReviewMedia)
            .Include(r => r.Package)
            .Where(r => r.PackagerId == packagerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Review>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Review?> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(r => r.BookingId == bookingId, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> HasUserReviewedBookingAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .AnyAsync(r => r.UserId == userId && r.BookingId == bookingId, cancellationToken);
}
