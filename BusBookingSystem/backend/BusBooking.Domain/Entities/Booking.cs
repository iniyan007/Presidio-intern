namespace BusBooking.Domain.Entities;
public class Booking
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TripId { get; set; }
    public Trip Trip { get; set; }

    public int SeatId { get; set; }

    public decimal BasePrice { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal TotalPrice { get; set; }

    public string Status { get; set; }

    public DateTime CreatedAt { get; set; }
}