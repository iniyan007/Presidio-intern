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

    // 🧑‍💼 OPERATOR and ADMIN
    [Authorize(Roles = "OPERATOR,ADMIN")]
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

    [Authorize(Roles = "OPERATOR,ADMIN")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(int id, CreateRouteRequest request)
    {
        var result = await _routeService.UpdateRoute(id, request);
        return Ok(result);
    }

    [Authorize(Roles = "OPERATOR,ADMIN")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        var result = await _routeService.DeleteRoute(id);
        return Ok(result);
    }
}