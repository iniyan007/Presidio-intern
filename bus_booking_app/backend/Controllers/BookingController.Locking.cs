using Microsoft.AspNetCore.Mvc;
using backend.Controllers; // Ensure access to requests if they are in this namespace

namespace backend.Controllers
{
    public partial class BookingController
    {
        [HttpPost("lock")]
        public async Task<IActionResult> LockSeats(LockSeatsRequest request)
        {
            var userId = GetUserId();
            var result = await _bookingService.LockSeatsAsync(userId, request.TripId, request.SeatNumbers);

            if (!result.Success)
            {
                if (result.ConflictingSeats != null)
                {
                    return BadRequest(new { message = result.Message, seats = result.ConflictingSeats });
                }
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { 
                message = result.Message, 
                bookingId = result.BookingId, 
                lockedUntil = result.LockedUntil 
            });
        }
    }
}
