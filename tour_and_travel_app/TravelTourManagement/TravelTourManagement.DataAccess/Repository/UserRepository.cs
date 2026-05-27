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
/// EF Core repository for the <see cref="User"/> entity.
/// </summary>
public class UserRepository : GenericRepository<User, Guid>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<User>> GetActiveUsersAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(u => u.IsActive)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);

    /// <inheritdoc />
    public async Task<User?> GetWithPackagerProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(u => u.PackagerUser)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    /// <inheritdoc />
    public async Task UpdateLastLoginAsync(
        Guid userId,
        DateTime loginTime,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbSet.FindAsync(new object?[] { userId }, cancellationToken);
        if (user is not null)
        {
            user.LastLoginAt = loginTime;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
