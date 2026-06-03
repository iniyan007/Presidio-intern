using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Packages;

namespace TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs;
using Microsoft.AspNetCore.Http;
using TravelTourManagement.DataAccess.DTOs.Packages;

public interface IPackageService
{
    Task<Guid> CreatePackageAsync(Guid userId, CreatePackageRequest request, List<IFormFile>? mediaFiles = null, CancellationToken cancellationToken = default);
    Task<string> UploadPackageMediaAsync(Guid userId, Guid packageId, Stream fileStream, string fileName, string contentType, string category, bool isPrimary, int displayOrder, string? caption, CancellationToken cancellationToken = default);
    Task DeletePackageAsync(Guid userId, Guid packageId, CancellationToken cancellationToken = default);
    
    Task UpdatePackageDetailsAsync(Guid userId, Guid packageId, UpdatePackageDetailsRequest request, CancellationToken cancellationToken = default);
    Task RepublishPackageAsync(Guid userId, Guid packageId, RepublishPackageRequest request, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<TravelTourManagement.DataAccess.DTOs.Packages.PackageSummaryResponse>> GetAllPublishedPackagesAsync(CancellationToken cancellationToken = default);

    Task<PagedResponse<TravelTourManagement.DataAccess.DTOs.Packages.PackageSummaryResponse>> SearchPackagesAsync(PackageSearchRequest request, CancellationToken cancellationToken = default);

    Task<TravelTourManagement.DataAccess.DTOs.Packages.PackageDetailResponse> GetPublishedPackageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PackageRevenueResponse> GetPackageRevenueAsync(Guid userId, string role, Guid packageId, CancellationToken cancellationToken = default);
}
