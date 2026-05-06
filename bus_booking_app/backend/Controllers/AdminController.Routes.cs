using Microsoft.AspNetCore.Mvc;
using backend.DTOs;

namespace backend.Controllers
{
    public partial class AdminController
    {
        [HttpGet("routes")]
        public async Task<IActionResult> GetRoutes()
        {
            var routes = await _adminService.GetRoutesAsync();
            return Ok(routes);
        }

        [HttpPost("routes")]
        public async Task<IActionResult> CreateRoute(CreateRouteRequest request)
        {
            var result = await _adminService.CreateRouteAsync(request);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message, route = result.Route });
        }

        [HttpDelete("routes/{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var result = await _adminService.DeleteRouteAsync(id);
            if (!result.Success) return NotFound(new { message = result.Message });
            return Ok(new { message = result.Message });
        }
    }
}
