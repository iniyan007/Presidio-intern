using Microsoft.AspNetCore.Mvc;
using backend.DTOs;

namespace backend.Controllers
{
    public partial class OperatorController
    {
        [HttpPost("trips")]
        public async Task<IActionResult> CreateTrip(CreateTripRequest request)
        {
            try {
                var opId = GetOperatorId();
                var result = await _operatorService.CreateTripAsync(opId, request);

                if (!result.Success) 
                {
                    if (result.Message.Contains("not found")) return NotFound(new { message = result.Message });
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { 
                    message = result.Message, 
                    tripDetails = result.TripData
                });
            } catch (Exception ex) {
                return StatusCode(500, new { message = "Internal server error during trip creation.", details = ex.Message });
            }
        }

        [HttpGet("trips")]
        public async Task<IActionResult> GetMyTrips()
        {
            var opId = GetOperatorId();
            var trips = await _operatorService.GetMyTripsAsync(opId);
            return Ok(trips);
        }

        [HttpDelete("trips/{tripId}")]
        public async Task<IActionResult> DeleteTrip(int tripId)
        {
            var opId = GetOperatorId();
            var result = await _operatorService.DeleteTripAsync(opId, tripId);
            if (!result.Success) return NotFound(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [HttpGet("trips/{tripId}/passengers")]
        public async Task<IActionResult> GetTripPassengers(int tripId)
        {
            var opId = GetOperatorId();
            var result = await _operatorService.GetTripPassengersAsync(opId, tripId);
            if (!result.Success) return NotFound(new { message = result.Message });
            return Ok(result.Data);
        }
    }
}
