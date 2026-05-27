using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="User"/> entity.
/// Extends the generic CRUD interface with user-domain-specific queries.
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{
    /// <summary>Retrieves a user by their unique email address.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Returns all users whose <c>IsActive</c> flag is <c>true</c>.</summary>
    Task<IReadOnlyList<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if a user with the specified email already exists,
    /// regardless of account status.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a user together with their associated <see cref="Packager"/> profile, if one exists.
    /// </summary>
    Task<User?> GetWithPackagerProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>LastLoginAt</c> timestamp for the specified user.</summary>
    Task UpdateLastLoginAsync(Guid userId, DateTime loginTime, CancellationToken cancellationToken = default);
}
