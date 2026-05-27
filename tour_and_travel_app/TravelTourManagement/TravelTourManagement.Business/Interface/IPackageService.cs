using System;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.Packages;

namespace TravelTourManagement.Business.Interface;

public interface IPackageService
{
    Task<Guid> CreatePackageAsync(Guid userId, CreatePackageRequest request, CancellationToken cancellationToken = default);
}
