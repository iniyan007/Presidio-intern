namespace BusBooking.Application.DTOs;

public class LockSeatRequest
{
    public int TripId { get; set; }
    public List<int> SeatIds { get; set; }
}