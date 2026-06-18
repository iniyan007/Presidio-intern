using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Packagers;

namespace TravelTourManagement.Business.Services;

public interface IPackagerService
{
    Task<PackagerResponse> ApplyToBecomePackagerAsync(Guid userId, ApplyPackagerRequest request, CancellationToken cancellationToken = default);
    Task<PackagerResponse> ApprovePackagerAsync(Guid packagerId, Guid adminUserId, CancellationToken cancellationToken = default);
    Task<PackagerResponse> RejectPackagerAsync(Guid packagerId, Guid adminUserId, string reason, CancellationToken cancellationToken = default);
    Task<IEnumerable<PackagerResponse>> GetPendingPackagersAsync(CancellationToken cancellationToken = default);
    Task<PackagerResponse> GetMyPackagerStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TravelTourManagement.DataAccess.DTOs.PagedResponse<PublicPackagerResponse>> GetPublicPackagersAsync(PackagerSearchRequest request, CancellationToken cancellationToken = default);
    Task<PublicPackagerResponse> GetPublicPackagerByNameAsync(string packagerName, CancellationToken cancellationToken = default);
    Task<IEnumerable<PackagerDocumentResponse>> GetPackagerDocumentsAsync(Guid packagerId, CancellationToken cancellationToken = default);
}
