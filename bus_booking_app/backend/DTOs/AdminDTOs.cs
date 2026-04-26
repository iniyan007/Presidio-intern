namespace backend.DTOs
{
    public class CreateRouteRequest
    {
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal Distance { get; set; }
    }
}
