using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusBooking.Infrastructure.Services;
using BusBooking.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class RouteController : ControllerBase
{
    private readonly RouteService _routeService;

    public RouteController(RouteService routeService)
    {
        _routeService = routeService;
    }

    // 🧑‍💼 ADMIN ONLY
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<IActionResult> CreateRoute(CreateRouteRequest request)
    {
        var result = await _routeService.CreateRoute(request);
        return Ok(result);
    }

    // 👤 PUBLIC (User can view)
    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetRoutes()
    {
        var routes = _routeService.GetRoutes();
        return Ok(routes);
    }
}