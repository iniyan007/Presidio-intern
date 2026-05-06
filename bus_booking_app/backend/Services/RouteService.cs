using backend.Interfaces;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class RouteService : IRouteService
    {
        private readonly ApplicationDbContext _context;

        public RouteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BusRoute>> GetRoutesAsync()
        {
            return await _context.Routes.ToListAsync();
        }
    }
}
