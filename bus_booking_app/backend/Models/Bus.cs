namespace backend.Models
{
    public class Bus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BusNumber { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int OperatorId { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User Operator { get; set; } = null!;
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
