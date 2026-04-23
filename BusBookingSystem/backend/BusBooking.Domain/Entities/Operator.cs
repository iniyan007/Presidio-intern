namespace BusBooking.Domain.Entities;

public class Operator
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public string CompanyName { get; set; }
    public string ContactNumber { get; set; }

    public string OperatingLocation { get; set; }

    public bool IsApproved { get; set; } = false;
    public bool IsActive { get; set; } = true;
}