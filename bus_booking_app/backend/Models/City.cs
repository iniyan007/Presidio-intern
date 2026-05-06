namespace backend.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<BusRoute> RoutesAsSource { get; set; } = new List<BusRoute>();
        public ICollection<BusRoute> RoutesAsDestination { get; set; } = new List<BusRoute>();
    }
}
