using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public partial class AdminController
    {
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _adminService.GetStatsAsync();
            return Ok(stats);
        }
    }
}
