namespace backend.Controllers
{
    public class LockSeatsRequest
    {
        public int TripId { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
    }

    public class ConfirmBookingRequest
    {
        public List<PassengerDetail> Passengers { get; set; } = new();
    }

    public class PassengerDetail
    {
        public string SeatNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Gender { get; set; } = "";
    }
}
