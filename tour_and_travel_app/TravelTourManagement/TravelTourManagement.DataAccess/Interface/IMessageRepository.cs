using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="Message"/> entity.
/// </summary>
public interface IMessageRepository : IRepository<Message, Guid>
{
    /// <summary>
    /// Returns all messages within the specified thread, ordered chronologically (oldest first).
    /// </summary>
    Task<IReadOnlyList<Message>> GetByThreadIdAsync(Guid threadId, CancellationToken cancellationToken = default);

    /// <summary>Returns all unread messages within the specified thread.</summary>
    Task<IReadOnlyList<Message>> GetUnreadByThreadIdAsync(Guid threadId, CancellationToken cancellationToken = default);

    /// <summary>Marks all messages in the given thread as read.</summary>
    Task MarkThreadMessagesAsReadAsync(Guid threadId, CancellationToken cancellationToken = default);

    /// <summary>Returns the count of unread messages across all threads for the given user (as a sender check).</summary>
    Task<int> GetUnreadCountByThreadAsync(Guid threadId, CancellationToken cancellationToken = default);
}
