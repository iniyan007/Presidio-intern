using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TripController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchTrips(
            [FromQuery] string? source, 
            [FromQuery] string? destination, 
            [FromQuery] DateTime? date)
        {
            Console.WriteLine($"[TripSearch] Source: {source}, Dest: {destination}, Date: {date}");
            
            var query = _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Where(t => t.Status == "Scheduled");

            if (!string.IsNullOrEmpty(source))
                query = query.Where(t => t.Route.Source.ToLower().Contains(source.ToLower()));

            if (!string.IsNullOrEmpty(destination))
                query = query.Where(t => t.Route.Destination.ToLower().Contains(destination.ToLower()));

            if (date.HasValue)
            {
                // Assume the user is searching in IST (UTC+5:30).
                // So midnight IST is 18:30 UTC of the previous day.
                var startOfDay = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc).AddHours(-5).AddMinutes(-30);
                var endOfDay = startOfDay.AddDays(1);
                query = query.Where(t => t.DepartureTime >= startOfDay && t.DepartureTime < endOfDay);
            }

            var trips = await query
                .OrderBy(t => t.DepartureTime)
                .Select(t => new {
                    t.Id,
                    t.DepartureTime,
                    t.ArrivalTime,
                    t.TicketPrice,
                    t.TotalPrice,
                    BusName = t.Bus.Name,
                    BusNumber = t.Bus.BusNumber,
                    Source = t.Route.Source,
                    Destination = t.Route.Destination,
                    Distance = t.Route.Distance,
                    AvailableSeats = t.Bus.TotalSeats - t.Bookings.Where(b => b.Status == "Confirmed").Sum(b => b.BookingSeats.Count)
                })
                .ToListAsync();

            Console.WriteLine($"[TripSearch] Found {trips.Count} trips.");
            return Ok(trips);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripDetails(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Include(t => t.Bookings).ThenInclude(b => b.BookingSeats)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            var bookedSeats = trip.Bookings
                .Where(b => b.Status == "Confirmed" || (b.Status == "Pending" && b.LockedUntil > DateTime.UtcNow))
                .SelectMany(b => b.BookingSeats)
                .Select(bs => bs.SeatNumber)
                .ToList();

            return Ok(new {
                trip.Id,
                trip.DepartureTime,
                trip.ArrivalTime,
                trip.TicketPrice,
                trip.TotalPrice,
                trip.PlatformFee,
                Bus = new {
                    trip.Bus.Name,
                    trip.Bus.BusNumber,
                    trip.Bus.TotalSeats
                },
                Route = new {
                    trip.Route.Source,
                    trip.Route.Destination
                },
                BookedSeats = bookedSeats
            });
        }
    }
}
