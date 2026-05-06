using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public partial class BookingController
    {
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = GetUserId();
            var bookings = await _bookingService.GetMyBookingsAsync(userId);
            return Ok(bookings);
        }
    }
}
