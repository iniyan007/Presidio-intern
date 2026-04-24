namespace BusBooking.Application.DTOs;

public class CreateBookingRequest
{
    public int TripId { get; set; }
    public List<int> SeatIds { get; set; }
}