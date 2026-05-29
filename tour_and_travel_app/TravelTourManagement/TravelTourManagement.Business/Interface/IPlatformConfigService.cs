using System;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.DataAccess.DTOs.PlatformConfig;

namespace TravelTourManagement.Business.Interface;

public interface IPlatformConfigService
{
    Task<PlatformConfigResponse> GetConfigAsync(CancellationToken cancellationToken = default);
    Task<PlatformConfigResponse> UpdateConfigAsync(Guid adminUserId, UpdatePlatformConfigRequest request, CancellationToken cancellationToken = default);
}
