namespace BusBooking.Domain.Entities;

public class SeatLock
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public string SeatNumber { get; set; }
    public int LockedByUserId { get; set; }
    public DateTime ExpiryTime { get; set; }
}