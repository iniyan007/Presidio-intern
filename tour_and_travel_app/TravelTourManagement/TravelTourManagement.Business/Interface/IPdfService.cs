using TravelTourManagement.DataAccess.Entities;

namespace TravelTourManagement.Business.Interface;

public interface IPdfService
{
    byte[] GenerateBookingTicketPdf(Booking booking);
}
