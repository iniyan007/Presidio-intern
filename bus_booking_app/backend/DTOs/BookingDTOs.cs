using System.Collections.Generic;
using backend.Controllers;

namespace backend.DTOs
{
    public class BookingHistoryDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public BookingTripDto Trip { get; set; } = new();
        public List<BookingSeatDto> Seats { get; set; } = new();
    }
    public class BookingTripDto
    {
        public DateTime DepartureTime { get; set; }
        public RouteDto Route { get; set; } = new();
        public BusDto Bus { get; set; } = new();
    }
    public class RouteDto { public string Source { get; set; } = ""; public string Destination { get; set; } = ""; }
    public class BusDto { public string Name { get; set; } = ""; public string BusNumber { get; set; } = ""; }
    public class BookingSeatDto { public string SeatNumber { get; set; } = ""; public string PassengerName { get; set; } = ""; public int PassengerAge { get; set; } public string PassengerGender { get; set; } = ""; }
}
