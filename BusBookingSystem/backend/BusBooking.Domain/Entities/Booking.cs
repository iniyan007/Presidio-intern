namespace BusBooking.Domain.Entities;
public class Booking
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TripId { get; set; }
    public Trip Trip { get; set; }
    public User User { get; set; }
    public List<BookingSeat> BookingSeats { get; set; }
    public decimal BasePrice { get; set; }     
    public decimal PlatformFee { get; set; }   
    public decimal TotalPrice { get; set; }     

    public string Status { get; set; }= "PENDING";

    public DateTime CreatedAt { get; set; }
}