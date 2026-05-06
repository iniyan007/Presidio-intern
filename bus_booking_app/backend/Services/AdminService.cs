using backend.Interfaces;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetStatsAsync()
        {
            var totalPlatformRevenue = await _context.Bookings
                .Include(b => b.BookingSeats)
                .Where(b => b.Status == "Confirmed")
                .SumAsync(b => b.Trip.PlatformFee * b.BookingSeats.Count);

            var pendingOperators = await _context.Operators
                .CountAsync(o => !o.IsApproved);

            var pendingBuses = await _context.Buses
                .CountAsync(b => !b.IsApproved);

            var totalBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Confirmed");

            return new
            {
                TotalPlatformRevenue = totalPlatformRevenue,
                PendingOperators = pendingOperators,
                PendingBuses = pendingBuses,
                TotalBookings = totalBookings
            };
        }

        public async Task<IEnumerable<BusRoute>> GetRoutesAsync()
        {
            return await _context.Routes.ToListAsync();
        }

        public async Task<(bool Success, string Message, BusRoute? Route)> CreateRouteAsync(CreateRouteRequest request)
        {
            var route = new BusRoute
            {
                Source = request.Source,
                Destination = request.Destination,
                Distance = request.Distance
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return (true, "Route created successfully.", route);
        }

        public async Task<(bool Success, string Message)> DeleteRouteAsync(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return (false, "Route not found.");

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return (true, "Route deleted.");
        }

        public async Task<IEnumerable<object>> GetPendingOperatorsAsync()
        {
            return await _context.Operators
                .Where(o => !o.IsApproved)
                .Select(o => new { o.Id, o.Name, o.Email, o.MobileNumber, o.CreatedAt })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetAllOperatorsAsync()
        {
            return await _context.Operators
                .Select(o => new { o.Id, o.Name, o.Email, o.MobileNumber, o.CreatedAt, o.IsApproved })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> ApproveOperatorAsync(int id)
        {
            var user = await _context.Operators.FindAsync(id);
            if (user == null) return (false, "Operator not found.");

            user.IsApproved = true;
            await _context.SaveChangesAsync();
            return (true, $"Operator '{user.Name}' approved successfully.");
        }

        public async Task<(bool Success, string Message)> RejectOperatorAsync(int id)
        {
            var user = await _context.Operators.FindAsync(id);
            if (user == null) return (false, "Operator not found.");

            _context.Operators.Remove(user);
            await _context.SaveChangesAsync();
            return (true, "Operator rejected and removed.");
        }

        public async Task<IEnumerable<object>> GetPendingBusesAsync()
        {
            return await _context.Buses
                .Include(b => b.Operator)
                .Where(b => !b.IsApproved)
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.BusNumber,
                    b.TotalSeats,
                    b.CreatedAt,
                    OperatorName = b.Operator.Name,
                    OperatorEmail = b.Operator.Email
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetAllBusesAsync()
        {
            return await _context.Buses
                .Include(b => b.Operator)
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.BusNumber,
                    b.TotalSeats,
                    b.CreatedAt,
                    b.IsApproved,
                    OperatorName = b.Operator.Name,
                    OperatorEmail = b.Operator.Email
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> ApproveBusAsync(int id)
        {
            var bus = await _context.Buses.FindAsync(id);
            if (bus == null) return (false, "Bus not found.");

            bus.IsApproved = true;
            await _context.SaveChangesAsync();
            return (true, $"Bus '{bus.Name}' ({bus.BusNumber}) approved.");
        }

        public async Task<(bool Success, string Message)> RejectBusAsync(int id)
        {
            var bus = await _context.Buses.FindAsync(id);
            if (bus == null) return (false, "Bus not found.");

            _context.Buses.Remove(bus);
            await _context.SaveChangesAsync();
            return (true, "Bus rejected and removed.");
        }
    }
}
