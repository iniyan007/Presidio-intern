public class CreateTripRequest
{
    public int BusId { get; set; }
    public int RouteId { get; set; }

    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
}