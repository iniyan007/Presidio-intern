using System.Linq;
using AutoMapper;
using TravelTourManagement.DataAccess.DTOs.Bookings;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Mappings;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        CreateMap<TravelDocument, TravelDocumentResponse>();

        CreateMap<BookingTraveler, BookingTravelerResponse>();

        CreateMap<Booking, BookingResponse>()
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()));
    }
}
