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
            .ConstructUsing((src, ctx) => new PackagerResponse(
                src.Id,
                src.UserId,
                src.CompanyName,
                src.BusinessLicenseNo,
                src.Description,
                src.ContactEmail,
                src.ContactPhone,
                src.WebsiteUrl,
                src.DeactivatedAt != null 
                    ? (src.ApprovedAt == null ? "Rejected" : "Deactivated")
                    : (src.ApprovedAt != null ? "Approved" : "Pending"),
                src.DeactivationReason,
                src.AvgRating,
                src.TotalReviews,
                src.CreatedAt
            ));

        CreateMap<Packager, PublicPackagerResponse>()
            .ForMember(dest => dest.TotalPackagesContributed, opt => opt.MapFrom(src => 
                src.Packages != null ? src.Packages.Count(p => p.Status == TravelTourManagement.DataAccess.Enums.PackageStatus.Published) : 0));
    }
}
