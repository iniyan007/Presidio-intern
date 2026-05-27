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
/// EF Core repository for the <see cref="Booking"/> entity.
/// </summary>
public class BookingRepository : GenericRepository<Booking, Guid>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<Booking?> GetByReferenceAsync(
        string bookingReference,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(b => b.BookingReference == bookingReference, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Booking>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Booking>> GetByPackageIdAsync(
        Guid packageId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(b => b.PackageId == packageId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Booking?> GetWithFullDetailsAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(b => b.BookingTravelers)
            .Include(b => b.Payments)
            .Include(b => b.TravelDocuments)
            .Include(b => b.Package)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Booking>> GetByTravelDateRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(b => b.TravelDate >= from && b.TravelDate <= to)
            .OrderBy(b => b.TravelDate)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<bool> ReferenceExistsAsync(
        string bookingReference,
        CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(b => b.BookingReference == bookingReference, cancellationToken);
}
