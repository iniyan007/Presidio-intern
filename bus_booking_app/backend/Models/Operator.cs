namespace backend.Models
{
    public class Operator : User
    {
        public Operator()
        {
            Role = "Operator";
        }

        public bool IsApproved { get; set; }

        // Navigation properties specific to Operator
        public ICollection<Bus> Buses { get; set; } = new List<Bus>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
