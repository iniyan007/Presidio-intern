using BusBooking.Domain.Entities;

public class Seat
{
    public int Id { get; set; }

    public int BusId { get; set; }
    public Bus Bus { get; set; }

    public string SeatNumber { get; set; } // A1, A2...

    public bool IsWindow { get; set; }
}