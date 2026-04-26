namespace backend.Models
{
    public class Trip
    {
        public int Id { get; set; }
        public int BusId { get; set; }
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal TicketPrice { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Scheduled";

        // Navigation properties
        public Bus Bus { get; set; } = null!;
        public BusRoute Route { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
