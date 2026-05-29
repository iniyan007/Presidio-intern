using AutoMapper;
using TravelTourManagement.DataAccess.DTOs.PlatformConfig;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Mappings;

public class PlatformConfigProfile : Profile
{
    public PlatformConfigProfile()
    {
        CreateMap<PlatformConfig, PlatformConfigResponse>();
    }
}
