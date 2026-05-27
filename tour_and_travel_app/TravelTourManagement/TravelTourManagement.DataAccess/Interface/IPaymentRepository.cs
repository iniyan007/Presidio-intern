using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="Payment"/> entity.
/// </summary>
public interface IPaymentRepository : IRepository<Payment, Guid>
{
    /// <summary>Returns all payments associated with the specified booking.</summary>
    Task<IReadOnlyList<Payment>> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a payment by the external gateway transaction ID,
    /// or <c>null</c> if no matching payment exists.
    /// </summary>
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if a transaction ID already exists,
    /// preventing duplicate payment records from gateway callbacks.
    /// </summary>
    Task<bool> TransactionExistsAsync(string transactionId, CancellationToken cancellationToken = default);
}
