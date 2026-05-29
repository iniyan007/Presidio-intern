using System.Linq;
using AutoMapper;
using TravelTourManagement.DataAccess.DTOs.Packagers;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Mappings;

public class PackagerProfile : Profile
{
    public PackagerProfile()
    {
        CreateMap<Packager, PackagerResponse>()
            .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => 
                src.DeactivatedAt != null 
                    ? (src.ApprovedAt == null ? "Rejected" : "Deactivated")
                    : (src.ApprovedAt != null ? "Approved" : "Pending")))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.DeactivationReason));

        CreateMap<Packager, PublicPackagerResponse>()
            .ForMember(dest => dest.TotalPackagesContributed, opt => opt.MapFrom(src => 
                src.Packages != null ? src.Packages.Count(p => p.Status == TravelTourManagement.DataAccess.Enums.PackageStatus.Published) : 0));
    }
}
