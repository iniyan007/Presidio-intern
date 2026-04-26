namespace backend.Models
{
    public class BookingSeat
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public int PassengerAge { get; set; }
        public string PassengerGender { get; set; } = string.Empty;

        // Navigation property
        public Booking Booking { get; set; } = null!;
    }
}
