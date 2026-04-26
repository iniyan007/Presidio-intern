namespace backend.Models
{
    public class BusRoute
    {
        public int Id { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
