using AutoMapper;
using TravelTourManagement.DataAccess.DTOs.Users;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResponse>()
            .ConstructUsing((src, ctx) => new UserResponse(
                src.Id,
                src.FullName,
                src.Email,
                src.Phone,
                src.ProfilePicture,
                src.IsActive,
                src.IsEmailVerified,
                src.PackagerUser != null && src.PackagerUser.ApprovedAt != null && src.PackagerUser.DeactivatedAt == null
            ));
    }
}
