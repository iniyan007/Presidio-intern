using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public partial class TripController
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripDetails(int id)
        {
            var tripDetails = await _tripService.GetTripDetailsAsync(id);
            if (tripDetails == null) return NotFound();
            return Ok(tripDetails);
        }
    }
}
