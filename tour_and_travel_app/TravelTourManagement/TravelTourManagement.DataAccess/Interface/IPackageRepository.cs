using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.DataAccess.Interface;
using TravelTourManagement.DataAccess.DTOs.Packages;

/// <summary>
/// Repository contract for the <see cref="Package"/> entity.
/// Extends the generic CRUD interface with package-domain-specific queries.
/// </summary>
public interface IPackageRepository : IRepository<Package, Guid>
{
    /// <summary>Returns all packages marked as featured (<c>IsFeatured = true</c>).</summary>
    Task<IReadOnlyList<Package>> GetFeaturedPackagesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all packages belonging to the specified packager.</summary>
    Task<IReadOnlyList<Package>> GetByPackagerIdAsync(Guid packagerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a case-insensitive partial match search on <c>Destination</c>,
    /// <c>Country</c>, <c>City</c>, and <c>Title</c>.
    /// </summary>
    Task<IReadOnlyList<Package>> SearchAsync(string keyword, CancellationToken cancellationToken = default);

    /// <summary>Returns all packages whose destination matches the given value (exact, case-insensitive).</summary>
    Task<IReadOnlyList<Package>> GetByDestinationAsync(string destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the package together with its itinerary days, highlights, inclusions,
    /// media, seasonal pricing, packager, and reviews.
    /// </summary>
    Task<Package?> GetWithFullDetailsAsync(Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicitly deletes all nested collections of a package from the database context.
    /// This avoids EF Core graph tracking issues when replacing collections.
    /// </summary>
    Task DeletePackageCollectionsAsync(Package package, bool preservePublishedData = false, CancellationToken cancellationToken = default);

    
    Task<IReadOnlyList<Package>> GetAllPublishedWithFullDetailsAsync(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Package> Packages, int TotalCount)> SearchPackagesAsync(PackageSearchRequest request, CancellationToken cancellationToken = default);

    Task<List<string>> GetDistinctCountriesAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetDistinctDestinationsAsync(string? country = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves available packages by date range. for booking within the given travel date range
    /// (i.e. at least one active seasonal pricing slot covers the range).
    /// </summary>
    Task<IReadOnlyList<Package>> GetAvailableByDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task<Package> CreatePackageWithDetailsAsync(
        Package package,
        CancellationToken cancellationToken = default);

    Task AddPackageMediaAsync(PackageMedium media, CancellationToken cancellationToken = default);
}
