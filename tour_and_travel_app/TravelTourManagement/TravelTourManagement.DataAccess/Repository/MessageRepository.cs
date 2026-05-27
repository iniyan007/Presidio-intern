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
/// EF Core repository for the <see cref="Message"/> entity.
/// </summary>
public class MessageRepository : GenericRepository<Message, Guid>, IMessageRepository
{
    public MessageRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Message>> GetByThreadIdAsync(
        Guid threadId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(m => m.ThreadId == threadId)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Message>> GetUnreadByThreadIdAsync(
        Guid threadId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(m => m.ThreadId == threadId && !m.IsRead)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task MarkThreadMessagesAsReadAsync(
        Guid threadId,
        CancellationToken cancellationToken = default)
    {
        var unread = await _dbSet
            .Where(m => m.ThreadId == threadId && !m.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count > 0)
        {
            foreach (var m in unread)
                m.IsRead = true;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountByThreadAsync(
        Guid threadId,
        CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(m => m.ThreadId == threadId && !m.IsRead, cancellationToken);
}
