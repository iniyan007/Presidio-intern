using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using backend.Data;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Operator")]
    public class OperatorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly backend.Services.IEmailService _emailService;

        public OperatorController(ApplicationDbContext context, backend.Services.IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private int GetOperatorId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        // --- BUSES ---

        [HttpPost("buses")]
        public async Task<IActionResult> AddBus(CreateBusRequest request)
        {
            var opId = GetOperatorId();
            if (await _context.Buses.AnyAsync(b => b.BusNumber == request.BusNumber))
                return BadRequest(new { message = "Bus number already exists." });

            var bus = new Bus
            {
                Name = request.Name,
                BusNumber = request.BusNumber,
                TotalSeats = request.TotalSeats,
                OperatorId = opId,
                IsApproved = false // Requires Admin Approval
            };

            _context.Buses.Add(bus);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Bus added successfully and is pending admin approval.", bus });
        }

        [HttpGet("buses")]
        public async Task<IActionResult> GetMyBuses()
        {
            var opId = GetOperatorId();
            var buses = await _context.Buses.Where(b => b.OperatorId == opId).ToListAsync();
            
            foreach(var b in buses) {
                Console.WriteLine($"[OperatorController] Bus {b.BusNumber}: IsApproved = {b.IsApproved}");
            }

            return Ok(buses);
        }

        // --- TRIPS ---

        [HttpPost("trips")]
        public async Task<IActionResult> CreateTrip(CreateTripRequest request)
        {
            try {
                var opId = GetOperatorId();
                var bus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == request.BusId && b.OperatorId == opId);
                
                if (bus == null) return NotFound(new { message = "Bus not found." });
                if (!bus.IsApproved) return BadRequest(new { message = "Cannot schedule a trip with an unapproved bus." });

                var route = await _context.Routes.FindAsync(request.RouteId);
                if (route == null) return NotFound(new { message = "Route not found." });

                if (request.DepartureTime >= request.ArrivalTime)
                {
                    return BadRequest(new { message = "Departure time must be earlier than arrival time." });
                }

                // Admin decides platform fee globally or we can set it fixed for now, e.g., 50 rs
                decimal platformFee = 50.0m; 

                var trip = new Trip
                {
                    BusId = request.BusId,
                    RouteId = request.RouteId,
                    DepartureTime = request.DepartureTime.ToUniversalTime(),
                    ArrivalTime = request.ArrivalTime.ToUniversalTime(),
                    TicketPrice = request.TicketPrice,
                    PlatformFee = platformFee,
                    TotalPrice = request.TicketPrice + platformFee,
                    Status = "Scheduled"
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Trip scheduled successfully.", 
                    tripId = trip.Id,
                    busId = trip.BusId,
                    routeId = trip.RouteId,
                    departureTime = trip.DepartureTime,
                    arrivalTime = trip.ArrivalTime,
                    totalPrice = trip.TotalPrice
                });
            } catch (Exception ex) {
                Console.WriteLine($"[CreateTrip Error] {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"[Inner] {ex.InnerException.Message}");
                return StatusCode(500, new { message = "Internal server error during trip creation.", details = ex.Message });
            }
        }

        [HttpGet("trips")]
        public async Task<IActionResult> GetMyTrips()
        {
            var opId = GetOperatorId();

            var now = DateTime.UtcNow;
            var tripsToComplete = await _context.Trips
                .Where(t => t.Bus.OperatorId == opId && t.Status == "Scheduled" && t.ArrivalTime <= now)
                .ToListAsync();

            if (tripsToComplete.Any())
            {
                foreach (var t in tripsToComplete)
                {
                    t.Status = "Completed";
                }
                await _context.SaveChangesAsync();
            }

            var trips = await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Where(t => t.Bus.OperatorId == opId)
                .OrderByDescending(t => t.DepartureTime)
                .Select(t => new {
                    t.Id,
                    t.DepartureTime,
                    t.ArrivalTime,
                    t.TicketPrice,
                    t.Status,
                    Bus = new {
                        t.Bus.Name,
                        t.Bus.BusNumber,
                        t.Bus.TotalSeats
                    },
                    Route = new {
                        t.Route.Source,
                        t.Route.Destination
                    },
                    AvailableSeats = t.Bus.TotalSeats - t.Bookings.Where(b => b.Status == "Confirmed").SelectMany(b => b.BookingSeats).Count()
                })
                .ToListAsync();

            return Ok(trips);
        }

        [HttpDelete("trips/{tripId}")]
        public async Task<IActionResult> DeleteTrip(int tripId)
        {
            var opId = GetOperatorId();
            var trip = await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Include(t => t.Bookings)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(t => t.Id == tripId && t.Bus.OperatorId == opId);

            if (trip == null) return NotFound(new { message = "Trip not found or unauthorized." });

            // Mark trip as cancelled
            trip.Status = "Cancelled";

            // Find all confirmed bookings
            var confirmedBookings = trip.Bookings.Where(b => b.Status == "Confirmed").ToList();
            
            foreach (var booking in confirmedBookings)
            {
                booking.Status = "Cancelled"; // Or RefundPending
                
                // Send email
                string subject = $"Trip Cancellation Notice: {trip.Route.Source} to {trip.Route.Destination}";
                string body = $@"
Dear {booking.User.Name},

Unfortunately, we must inform you that your upcoming trip from {trip.Route.Source} to {trip.Route.Destination} on {trip.DepartureTime:MMM dd, yyyy h:mm tt} has been cancelled by the operator ({trip.Bus.Name}).

We sincerely apologize for any inconvenience this may cause.

A full refund of ₹{booking.TotalAmount} will be processed back to your original payment method within 3-5 business days.

If you have any questions, please contact support.

Regards,
BusBooking Team
";
                await _emailService.SendEmailAsync(booking.User.Email, subject, body);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Trip cancelled successfully and affected passengers have been notified." });
        }

        [HttpGet("trips/{tripId}/passengers")]
        public async Task<IActionResult> GetTripPassengers(int tripId)
        {
            var opId = GetOperatorId();
            var trip = await _context.Trips.Include(t => t.Bus).FirstOrDefaultAsync(t => t.Id == tripId);
            
            if (trip == null || trip.Bus.OperatorId != opId)
                return NotFound(new { message = "Trip not found." });

            var passengers = await _context.BookingSeats
                .Include(bs => bs.Booking)
                .ThenInclude(b => b.User)
                .Where(bs => bs.Booking.TripId == tripId && bs.Booking.Status == "Confirmed")
                .Select(bs => new {
                    bs.SeatNumber,
                    bs.PassengerName,
                    bs.PassengerAge,
                    bs.PassengerGender,
                    bs.Booking.User.MobileNumber
                })
                .ToListAsync();

            var totalRevenue = await _context.BookingSeats
                .Include(bs => bs.Booking)
                .Where(bs => bs.Booking.TripId == tripId && bs.Booking.Status == "Confirmed")
                .SumAsync(bs => trip.TicketPrice);

            return Ok(new {
                Passengers = passengers,
                TotalRevenue = totalRevenue
            });
        }

        // --- REVENUE & EXPENSES ---

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueStats()
        {
            var opId = GetOperatorId();
            var myTrips = await _context.Trips.Where(t => t.Bus.OperatorId == opId).Select(t => t.Id).ToListAsync();

            var totalRevenue = await _context.Bookings
                .Where(b => myTrips.Contains(b.TripId) && b.Status == "Confirmed")
                .SumAsync(b => b.TotalAmount); // Note: technically operator revenue is (TotalAmount - PlatformFee), but TotalAmount is fine for now, or we can calculate it precisely.
            
            // Precise Operator Revenue = TicketPrice * SeatsBooked
            var operatorRevenue = await _context.BookingSeats
                .Include(bs => bs.Booking)
                .ThenInclude(b => b.Trip)
                .Where(bs => myTrips.Contains(bs.Booking.TripId) && bs.Booking.Status == "Confirmed")
                .SumAsync(bs => bs.Booking.Trip.TicketPrice);

            var totalExpenses = await _context.Expenses
                .Where(e => e.OperatorId == opId)
                .SumAsync(e => e.Amount);

            return Ok(new
            {
                TotalRevenue = operatorRevenue,
                TotalExpenses = totalExpenses,
                NetProfit = operatorRevenue - totalExpenses
            });
        }

        [HttpPost("expenses")]
        public async Task<IActionResult> AddExpense(CreateExpenseRequest request)
        {
            var opId = GetOperatorId();
            var trip = await _context.Trips.Include(t => t.Bus).FirstOrDefaultAsync(t => t.Id == request.TripId);
            if (trip == null || trip.Bus.OperatorId != opId)
                return NotFound(new { message = "Trip not found." });

            var expense = new Expense
            {
                OperatorId = opId,
                TripId = request.TripId,
                Description = request.Description,
                Amount = request.Amount
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Expense recorded successfully." });
        }
    }
}
