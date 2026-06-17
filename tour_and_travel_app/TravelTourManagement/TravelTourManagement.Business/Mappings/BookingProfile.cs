using System.Linq;
using AutoMapper;
using TravelTourManagement.DataAccess.DTOs.Bookings;
using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Mappings;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        CreateMap<TravelDocument, TravelDocumentResponse>()
            .ConstructUsing(src => new TravelDocumentResponse(
                src.Id,
                src.DocumentType,
                src.FilePath,
                src.FileName,
                src.UploadedAt,
                src.Status.ToString(),
                src.RejectionReason
            ));

        CreateMap<BookingTraveler, BookingTravelerResponse>()
            .ConstructUsing((src, context) => new BookingTravelerResponse(
                src.Id,
                src.FullName,
                src.PassportNumber,
                src.DateOfBirth,
                src.Nationality,
                src.Age,
                src.Gender,
                src.MealPreference,
                src.AadharCardNumber,
                src.IsPrimary,
                src.TravelDocuments != null ? src.TravelDocuments.Select(d => context.Mapper.Map<TravelDocumentResponse>(d)).ToList() : new System.Collections.Generic.List<TravelDocumentResponse>()
            ));

        CreateMap<Booking, BookingResponse>()
            .ConstructUsing((src, context) => new BookingResponse(
                src.Id,
                src.UserId,
                src.PackageId,
                src.BookingReference,
                src.AdultCount,
                src.ChildCount,
                src.InfantCount,
                src.TotalAmount,
                src.PaidAmount,
                src.Status.ToString(),
                src.PaymentStatus.ToString(),
                src.TravelDate,
                src.ReturnDate,
                src.SpecialRequests,
                src.BookedAt,
                src.CancelledAt,
                src.CancellationReason,
                src.BookingTravelers != null ? src.BookingTravelers.Select(t => context.Mapper.Map<BookingTravelerResponse>(t)).ToList() : new System.Collections.Generic.List<BookingTravelerResponse>()
            ));
    }
}
