using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── DASHBOARD ────────────────────────────────────────────────

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalPlatformRevenue = await _context.Bookings
                .Include(b => b.BookingSeats)
                .Where(b => b.Status == "Confirmed")
                .SumAsync(b => b.Trip.PlatformFee * b.BookingSeats.Count);

            var pendingOperators = await _context.Users
                .CountAsync(u => u.Role == "Operator" && !u.IsApproved);

            var pendingBuses = await _context.Buses
                .CountAsync(b => !b.IsApproved);

            var totalBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Confirmed");

            return Ok(new
            {
                TotalPlatformRevenue = totalPlatformRevenue,
                PendingOperators = pendingOperators,
                PendingBuses = pendingBuses,
                TotalBookings = totalBookings
            });
        }

        // ─── ROUTES ───────────────────────────────────────────────────

        [HttpGet("routes")]
        public async Task<IActionResult> GetRoutes()
        {
            var routes = await _context.Routes.ToListAsync();
            return Ok(routes);
        }

        [HttpPost("routes")]
        public async Task<IActionResult> CreateRoute(CreateRouteRequest request)
        {
            var route = new BusRoute
            {
                Source = request.Source,
                Destination = request.Destination,
                Distance = request.Distance
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Route created successfully.", route });
        }

        [HttpDelete("routes/{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound(new { message = "Route not found." });

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Route deleted." });
        }

        // ─── OPERATOR APPROVALS ───────────────────────────────────────

        [HttpGet("pending-operators")]
        public async Task<IActionResult> GetPendingOperators()
        {
            var operatorsQuery = _context.Users
                .Where(u => u.Role.ToLower() == "operator" && !u.IsApproved);
            
            var count = await operatorsQuery.CountAsync();
            Console.WriteLine($"[AdminController] Found {count} pending operators in database.");

            var operators = await operatorsQuery
                .Select(u => new { u.Id, u.Name, u.Email, u.MobileNumber, u.CreatedAt })
                .ToListAsync();
            return Ok(operators);
        }

        [HttpPost("operators/{id}/approve")]
        public async Task<IActionResult> ApproveOperator(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Operator")
                return NotFound(new { message = "Operator not found." });

            user.IsApproved = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Operator '{user.Name}' approved successfully." });
        }

        [HttpDelete("operators/{id}/reject")]
        public async Task<IActionResult> RejectOperator(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Operator")
                return NotFound(new { message = "Operator not found." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Operator rejected and removed." });
        }

        [HttpGet("operators")]
        public async Task<IActionResult> GetAllOperators()
        {
            var operators = await _context.Users
                .Where(u => u.Role.ToLower() == "operator")
                .Select(u => new { u.Id, u.Name, u.Email, u.MobileNumber, u.CreatedAt, u.IsApproved })
                .ToListAsync();
            return Ok(operators);
        }

        // ─── BUS APPROVALS ────────────────────────────────────────────

        [HttpGet("pending-buses")]
        public async Task<IActionResult> GetPendingBuses()
        {
            var buses = await _context.Buses
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
            return Ok(buses);
        }

        [HttpGet("buses")]
        public async Task<IActionResult> GetAllBuses()
        {
            var buses = await _context.Buses
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
            return Ok(buses);
        }

        [HttpPost("buses/{id}/approve")]
        public async Task<IActionResult> ApproveBus(int id)
        {
            var bus = await _context.Buses.FindAsync(id);
            if (bus == null) return NotFound(new { message = "Bus not found." });

            bus.IsApproved = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Bus '{bus.Name}' ({bus.BusNumber}) approved." });
        }

        [HttpDelete("buses/{id}/reject")]
        public async Task<IActionResult> RejectBus(int id)
        {
            var bus = await _context.Buses.FindAsync(id);
            if (bus == null) return NotFound(new { message = "Bus not found." });

            _context.Buses.Remove(bus);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Bus rejected and removed." });
        }
    }
}
