namespace BusBooking.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TripId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }

    public User User { get; set; }
}