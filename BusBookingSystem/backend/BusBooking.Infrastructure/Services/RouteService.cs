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

    public async Task<string> UpdateRoute(int id, CreateRouteRequest request)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null) return "Route not found";

        route.Source = request.Source;
        route.Destination = request.Destination;
        await _context.SaveChangesAsync();
        
        return "Route updated successfully";
    }

    public async Task<string> DeleteRoute(int id)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null) return "Route not found";

        _context.Routes.Remove(route);
        await _context.SaveChangesAsync();
        
        return "Route deleted successfully";
    }
}