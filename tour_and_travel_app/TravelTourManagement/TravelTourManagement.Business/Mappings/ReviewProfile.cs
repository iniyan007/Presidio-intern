using AutoMapper;
using TravelTourManagement.DataAccess.DTOs.Reviews;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Mappings;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<Review, ReviewResponse>()
            .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Anonymous"))
            .ForMember(dest => dest.Media, opt => opt.MapFrom(src => src.ReviewMedia));

        CreateMap<ReviewMedium, ReviewMediaResponse>();
    }
}
