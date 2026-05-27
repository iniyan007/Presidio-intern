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
/// EF Core repository for the <see cref="MessageThread"/> entity.
/// </summary>
public class MessageThreadRepository : GenericRepository<MessageThread, Guid>, IMessageThreadRepository
{
    public MessageThreadRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MessageThread>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(mt => mt.Packager)
            .Include(mt => mt.Package)
            .Where(mt => mt.UserId == userId)
            .OrderByDescending(mt => mt.LastMessageAt ?? mt.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<MessageThread>> GetByPackagerIdAsync(
        Guid packagerId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(mt => mt.User)
            .Include(mt => mt.Package)
            .Where(mt => mt.PackagerId == packagerId)
            .OrderByDescending(mt => mt.LastMessageAt ?? mt.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<MessageThread?> GetByUserAndPackagerAsync(
        Guid userId,
        Guid packagerId,
        Guid? packageId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(mt =>
                mt.UserId == userId &&
                mt.PackagerId == packagerId &&
                mt.PackageId == packageId,
                cancellationToken);

    /// <inheritdoc />
    public async Task<MessageThread?> GetWithMessagesAsync(
        Guid threadId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(mt => mt.User)
            .Include(mt => mt.Packager)
            .Include(mt => mt.Package)
            .Include(mt => mt.Messages.OrderByDescending(m => m.SentAt).Take(50)) // fetch latest 50
            .FirstOrDefaultAsync(mt => mt.Id == threadId, cancellationToken);
}
