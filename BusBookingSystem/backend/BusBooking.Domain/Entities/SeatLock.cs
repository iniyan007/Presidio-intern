public class SeatLock
{
    public int Id { get; set; }

    public int TripId { get; set; }
    public int SeatId { get; set; }

    public int UserId { get; set; }

    public DateTime LockedAt { get; set; }
}