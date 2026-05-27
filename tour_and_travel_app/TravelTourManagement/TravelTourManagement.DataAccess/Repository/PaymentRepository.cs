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
/// EF Core repository for the <see cref="Payment"/> entity.
/// </summary>
public class PaymentRepository : GenericRepository<Payment, Guid>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Payment>> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Payment?> GetByTransactionIdAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> TransactionExistsAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(p => p.TransactionId == transactionId, cancellationToken);
}
