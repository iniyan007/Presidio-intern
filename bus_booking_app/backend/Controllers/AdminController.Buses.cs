using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public partial class AdminController
    {
        [HttpGet("pending-buses")]
        public async Task<IActionResult> GetPendingBuses()
        {
            var buses = await _adminService.GetPendingBusesAsync();
            return Ok(buses);
        }

        [HttpGet("buses")]
        public async Task<IActionResult> GetAllBuses()
        {
            var buses = await _adminService.GetAllBusesAsync();
            return Ok(buses);
        }

        [HttpPost("buses/{id}/approve")]
        public async Task<IActionResult> ApproveBus(int id)
        {
            var result = await _adminService.ApproveBusAsync(id);
            if (!result.Success) return NotFound(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [HttpDelete("buses/{id}/reject")]
        public async Task<IActionResult> RejectBus(int id)
        {
            var result = await _adminService.RejectBusAsync(id);
            if (!result.Success) return NotFound(new { message = result.Message });
            return Ok(new { message = result.Message });
        }
    }
}
