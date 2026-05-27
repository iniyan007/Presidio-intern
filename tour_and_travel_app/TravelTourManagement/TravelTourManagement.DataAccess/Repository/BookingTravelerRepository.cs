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
/// EF Core repository for the <see cref="BookingTraveler"/> entity.
/// </summary>
public class BookingTravelerRepository : GenericRepository<BookingTraveler, Guid>, IBookingTravelerRepository
{
    public BookingTravelerRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BookingTraveler>> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(bt => bt.BookingId == bookingId)
            .OrderByDescending(bt => bt.IsPrimary)
            .ThenBy(bt => bt.FullName)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<BookingTraveler?> GetPrimaryTravelerAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(bt => bt.BookingId == bookingId && bt.IsPrimary, cancellationToken);
}
