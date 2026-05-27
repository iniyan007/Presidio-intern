using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="MessageThread"/> entity.
/// </summary>
public interface IMessageThreadRepository : IRepository<MessageThread, Guid>
{
    /// <summary>Returns all message threads initiated by the specified user.</summary>
    Task<IReadOnlyList<MessageThread>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns all message threads associated with the specified packager.</summary>
    Task<IReadOnlyList<MessageThread>> GetByPackagerIdAsync(Guid packagerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the unique thread between a user and a packager (optionally scoped to a package),
    /// or returns <c>null</c> if no such thread exists yet.
    /// </summary>
    Task<MessageThread?> GetByUserAndPackagerAsync(
        Guid userId,
        Guid packagerId,
        Guid? packageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the thread together with its most recent messages,
    /// ordered by <c>LastMessageAt</c> descending.
    /// </summary>
    Task<MessageThread?> GetWithMessagesAsync(Guid threadId, CancellationToken cancellationToken = default);
}
