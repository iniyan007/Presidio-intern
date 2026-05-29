using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.PlatformConfig;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Interface;
using AutoMapper;

namespace TravelTourManagement.Business.Services;

public class PlatformConfigService : IPlatformConfigService
{
    private readonly IRepository<PlatformConfig, Guid> _platformConfigRepository;
    private readonly IMapper _mapper;

    public PlatformConfigService(IRepository<PlatformConfig, Guid> platformConfigRepository, IMapper mapper)
    {
        _platformConfigRepository = platformConfigRepository;
        _mapper = mapper;
    }

    public async Task<PlatformConfigResponse> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        var configs = await _platformConfigRepository.GetAllAsync(cancellationToken);
        var config = configs.FirstOrDefault();

        if (config == null)
        {
            config = new PlatformConfig
            {
                PlatformFeePercent = 5.0m,
                GstPercent = 10.0m,
                UpdatedAt = DateTime.UtcNow,
                Note = "Default initial configuration"
            };
            await _platformConfigRepository.AddAsync(config, cancellationToken);
        }

        return _mapper.Map<PlatformConfigResponse>(config);
    }

    public async Task<PlatformConfigResponse> UpdateConfigAsync(Guid adminUserId, UpdatePlatformConfigRequest request, CancellationToken cancellationToken = default)
    {
        var configs = await _platformConfigRepository.GetAllAsync(cancellationToken);
        var config = configs.FirstOrDefault();

        if (config == null)
        {
            config = new PlatformConfig
            {
                PlatformFeePercent = request.PlatformFeePercent,
                GstPercent = request.GstPercent,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = adminUserId,
                Note = request.Note
            };
            await _platformConfigRepository.AddAsync(config, cancellationToken);
        }
        else
        {
            config.PlatformFeePercent = request.PlatformFeePercent;
            config.GstPercent = request.GstPercent;
            config.UpdatedAt = DateTime.UtcNow;
            config.UpdatedBy = adminUserId;
            config.Note = request.Note;
            await _platformConfigRepository.UpdateAsync(config, cancellationToken);
        }

        return _mapper.Map<PlatformConfigResponse>(config);
    }

    
}
