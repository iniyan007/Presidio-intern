namespace backend.DTOs
{
    public class CreateBusRequest
    {
        public string Name { get; set; } = string.Empty;
        public string BusNumber { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
    }

    public class CreateTripRequest
    {
        public int BusId { get; set; }
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal TicketPrice { get; set; }
    }

    public class CreateExpenseRequest
    {
        public int TripId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
