using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;

/// <summary>
/// Repository contract for the <see cref="Packager"/> entity.
/// Extends the generic CRUD interface with packager-domain-specific queries.
/// </summary>
public interface IPackagerRepository : IRepository<Packager, Guid>
{
    /// <summary>
    /// Retrieves the packager profile associated with a given <see cref="User"/> ID,
    /// or <c>null</c> if no packager profile exists for that user.
    /// </summary>
    Task<Packager?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns all packagers whose approval status is <c>approved</c>.</summary>
    Task<IReadOnlyList<Packager>> GetApprovedPackagersAsync(string? searchTerm = null, string? sortOrder = null, CancellationToken cancellationToken = default);

    /// <summary>Returns all packagers whose status is <c>deactivated</c>.</summary>
    Task<IReadOnlyList<Packager>> GetDeactivatedPackagersAsync(string? searchTerm = null, string? sortOrder = null, CancellationToken cancellationToken = default);

    /// <summary>Returns all packagers whose status is <c>pending</c> approval.</summary>
    Task<IReadOnlyList<Packager>> GetPendingApprovalAsync(string? searchTerm = null, string? sortOrder = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a packager by their company name.
    /// </summary>
    Task<Packager?> GetByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the packager together with their full list of <see cref="PackagerDocument"/> entities.
    /// </summary>
    Task<Packager?> GetWithDocumentsAsync(Guid packagerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the packager together with their full list of <see cref="Package"/> entities.
    /// </summary>
    Task<Packager?> GetWithPackagesAsync(Guid packagerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries public packagers with pagination and search.
    /// </summary>
    Task<(IReadOnlyList<Packager> Packagers, int TotalCount)> SearchPublicPackagersAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);


    /// <summary>
    /// Returns <c>true</c> if a packager profile already exists for the given user.
    /// </summary>
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status column in the database directly using raw SQL since the Postgres Enum is not mapped in the entity.
    /// </summary>
    Task UpdateStatusRawAsync(Guid packagerId, string status, CancellationToken cancellationToken = default);
}
