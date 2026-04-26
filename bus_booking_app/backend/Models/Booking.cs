namespace backend.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime LockedUntil { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Trip Trip { get; set; } = null!;
        public User User { get; set; } = null!;
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
