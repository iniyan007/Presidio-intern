using BusBooking.Domain.Entities;
using BusBooking.Infrastructure.Data;
using BusBooking.Application.DTOs;

namespace BusBooking.Infrastructure.Services;

public class RouteService
{
    private readonly ApplicationDbContext _context;

    public RouteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreateRoute(CreateRouteRequest request)
    {
        var route = new Route
        {
            Source = request.Source,
            Destination = request.Destination
        };

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        return "Route created successfully";
    }

    public List<Route> GetRoutes()
    {
        return _context.Routes.ToList();
    }
}