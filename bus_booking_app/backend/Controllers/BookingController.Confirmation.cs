using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public partial class BookingController
    {
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmBooking(int id, [FromBody] ConfirmBookingRequest request)
        {
            var userId = GetUserId();
            var result = await _bookingService.ConfirmBookingAsync(userId, id, request.Passengers);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message, bookingId = result.BookingId });
        }
    }
}
