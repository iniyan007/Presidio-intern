using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using TravelTourManagement.Business.Extensions;
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
    private readonly IDistributedCache _cache;
    private const string CacheKey = "PlatformConfig";

    public PlatformConfigService(IRepository<PlatformConfig, Guid> platformConfigRepository, IMapper mapper, IDistributedCache cache)
    {
        _platformConfigRepository = platformConfigRepository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<PlatformConfigResponse> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        // 1. Check cache
        var cachedConfig = await _cache.GetRecordAsync<PlatformConfigResponse>(CacheKey, cancellationToken);
        if (cachedConfig != null)
        {
            return cachedConfig;
        }

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

        var response = _mapper.Map<PlatformConfigResponse>(config);
        
        // 2. Set cache (60 minutes)
        await _cache.SetRecordAsync(CacheKey, response, TimeSpan.FromMinutes(60), null, cancellationToken);
        
        return response;
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

        var response = _mapper.Map<PlatformConfigResponse>(config);

        // 3. Invalidate/Update cache
        await _cache.SetRecordAsync(CacheKey, response, TimeSpan.FromMinutes(60), null, cancellationToken);

        return response;
    }

    
}
