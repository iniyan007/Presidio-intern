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
/// EF Core repository for the <see cref="Packager"/> entity.
/// </summary>
public class PackagerRepository : GenericRepository<Packager, Guid>, IPackagerRepository
{
    public PackagerRepository(ApplicationDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<Packager?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(pk => pk.UserId == userId, cancellationToken);

    /// <inheritdoc />
    /// <remarks>
    /// "Approved" is determined by <c>ApprovedAt != null</c> and <c>DeactivatedAt == null</c>.
    /// </remarks>
    public async Task<IReadOnlyList<Packager>> GetApprovedPackagersAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(pk => pk.ApprovedAt != null && pk.DeactivatedAt == null)
            .OrderByDescending(pk => pk.AvgRating)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    /// <remarks>
    /// "Pending" is determined by <c>ApprovedAt == null</c> and <c>DeactivatedAt == null</c>.
    /// </remarks>
    public async Task<IReadOnlyList<Packager>> GetPendingApprovalAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(pk => pk.ApprovedAt == null && pk.DeactivatedAt == null)
            .OrderBy(pk => pk.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Packager?> GetWithDocumentsAsync(
        Guid packagerId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(pk => pk.PackagerDocuments)
            .FirstOrDefaultAsync(pk => pk.Id == packagerId, cancellationToken);

    /// <inheritdoc />
    public async Task<Packager?> GetByCompanyNameAsync(
        string companyName,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(pk => pk.Packages)
            .FirstOrDefaultAsync(pk => pk.CompanyName.ToLower() == companyName.ToLower() && pk.ApprovedAt != null && pk.DeactivatedAt == null, cancellationToken);

    /// <inheritdoc />
    public async Task<Packager?> GetWithPackagesAsync(
        Guid packagerId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(pk => pk.Packages)
            .FirstOrDefaultAsync(pk => pk.Id == packagerId, cancellationToken);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Packager> Packagers, int TotalCount)> SearchPublicPackagersAsync(
        string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(pk => pk.ApprovedAt != null && pk.DeactivatedAt == null)
            .Include(pk => pk.Packages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(pk => pk.CompanyName.ToLower().Contains(term) || (pk.Description != null && pk.Description.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var packagers = await query
            .OrderByDescending(pk => pk.AvgRating)
            .ThenBy(pk => pk.CompanyName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (packagers, totalCount);
    }


    /// <inheritdoc />
    public async Task<bool> ExistsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(pk => pk.UserId == userId, cancellationToken);

    /// <inheritdoc />
    public async Task UpdateStatusRawAsync(Guid packagerId, string status, CancellationToken cancellationToken = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE packagers SET status = {0}::packager_status WHERE id = {1}", 
            new object[] { status, packagerId }, 
            cancellationToken);
    }
}
