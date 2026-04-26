namespace backend.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public int OperatorId { get; set; }
        public int TripId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User Operator { get; set; } = null!;
        public Trip Trip { get; set; } = null!;
    }
}
