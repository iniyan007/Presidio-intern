namespace BusBooking.Domain.Entities;

public class Trip
{
    public int Id { get; set; }

    public int BusId { get; set; }
    public Bus Bus { get; set; }

    public int RouteId { get; set; }
    public Route Route { get; set; }

    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }

    public bool IsActive { get; set; } = true;
}