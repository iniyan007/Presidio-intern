using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Packages;

namespace TravelTourManagement.Business.Interface;

public interface IPackageService
{
    Task<Guid> CreatePackageAsync(Guid userId, CreatePackageRequest request, CancellationToken cancellationToken = default);
    Task<string> UploadPackageMediaAsync(Guid userId, Guid packageId, Stream fileStream, string fileName, string contentType, string category, bool isPrimary, int displayOrder, string? caption, CancellationToken cancellationToken = default);
    Task DeletePackageAsync(Guid userId, Guid packageId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TravelTourManagement.DataAccess.DTOs.Packages.PackageSummaryResponse>> GetAllPublishedPackagesAsync(CancellationToken cancellationToken = default);
    Task<TravelTourManagement.DataAccess.DTOs.Packages.PackageDetailResponse> GetPublishedPackageByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
