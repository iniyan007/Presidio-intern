using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public partial class RouteController
    {
        [HttpGet]
        public async Task<IActionResult> GetRoutes()
        {
            var routes = await _routeService.GetRoutesAsync();
            return Ok(routes);
        }
    }
}
