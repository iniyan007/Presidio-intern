namespace BusBooking.Domain.Entities;

public class Bus
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int OperatorId { get; set; }
    public Operator Operator { get; set; }

    public int TotalSeats { get; set; }
    public decimal Price { get; set; }

    public bool IsApproved { get; set; } = false;
    public bool IsActive { get; set; } = true;
}