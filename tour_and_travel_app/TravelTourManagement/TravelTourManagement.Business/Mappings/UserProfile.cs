using AutoMapper;
using TravelTourManagement.DataAccess.DTOs.Users;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.IsPackager, opt => opt.MapFrom(src => 
                src.PackagerUser != null && src.PackagerUser.ApprovedAt != null && src.PackagerUser.DeactivatedAt == null));
    }
}
