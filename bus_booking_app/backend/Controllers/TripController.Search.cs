using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public partial class TripController
    {
        [HttpGet("search")]
        public async Task<IActionResult> SearchTrips(
            [FromQuery] string? source, 
            [FromQuery] string? destination, 
            [FromQuery] DateTime? date)
        {
            var trips = await _tripService.SearchTripsAsync(source, destination, date);
            return Ok(trips);
        }
    }
}
