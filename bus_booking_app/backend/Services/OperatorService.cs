using backend.Interfaces;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class OperatorService : IOperatorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public OperatorService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<(bool Success, string Message, Bus? Bus)> AddBusAsync(int operatorId, CreateBusRequest request)
        {
            if (await _context.Buses.AnyAsync(b => b.BusNumber == request.BusNumber))
                return (false, "Bus number already exists.", null);

            var bus = new Bus
            {
                Name = request.Name,
                BusNumber = request.BusNumber,
                TotalSeats = request.TotalSeats,
                OperatorId = operatorId,
                IsApproved = false
            };

            _context.Buses.Add(bus);
            await _context.SaveChangesAsync();
            return (true, "Bus added successfully and is pending admin approval.", bus);
        }

        public async Task<IEnumerable<Bus>> GetMyBusesAsync(int operatorId)
        {
            return await _context.Buses.Where(b => b.OperatorId == operatorId).ToListAsync();
        }

        public async Task<(bool Success, string Message, object? TripData)> CreateTripAsync(int operatorId, CreateTripRequest request)
        {
            var bus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == request.BusId && b.OperatorId == operatorId);
            if (bus == null) return (false, "Bus not found.", null);
            if (!bus.IsApproved) return (false, "Cannot schedule a trip with an unapproved bus.", null);

            var route = await _context.Routes.FindAsync(request.RouteId);
            if (route == null) return (false, "Route not found.", null);

            if (request.DepartureTime >= request.ArrivalTime)
            {
                return (false, "Departure time must be earlier than arrival time.", null);
            }

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

            var tripData = new { 
                tripId = trip.Id,
                busId = trip.BusId,
                routeId = trip.RouteId,
                departureTime = trip.DepartureTime,
                arrivalTime = trip.ArrivalTime,
                totalPrice = trip.TotalPrice
            };

            return (true, "Trip scheduled successfully.", tripData);
        }

        public async Task<IEnumerable<object>> GetMyTripsAsync(int operatorId)
        {
            var now = DateTime.UtcNow;
            var tripsToComplete = await _context.Trips
                .Where(t => t.Bus.OperatorId == operatorId && t.Status == "Scheduled" && t.ArrivalTime <= now)
                .ToListAsync();

            if (tripsToComplete.Any())
            {
                foreach (var t in tripsToComplete)
                {
                    t.Status = "Completed";
                }
                await _context.SaveChangesAsync();
            }

            return await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Where(t => t.Bus.OperatorId == operatorId)
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
        }

        public async Task<(bool Success, string Message)> DeleteTripAsync(int operatorId, int tripId)
        {
            var trip = await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Include(t => t.Bookings)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(t => t.Id == tripId && t.Bus.OperatorId == operatorId);

            if (trip == null) return (false, "Trip not found or unauthorized.");

            trip.Status = "Cancelled";

            var confirmedBookings = trip.Bookings.Where(b => b.Status == "Confirmed").ToList();
            
            foreach (var booking in confirmedBookings)
            {
                booking.Status = "Cancelled";
                
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
            return (true, "Trip cancelled successfully and affected passengers have been notified.");
        }

        public async Task<(bool Success, string Message, object? Data)> GetTripPassengersAsync(int operatorId, int tripId)
        {
            var trip = await _context.Trips.Include(t => t.Bus).FirstOrDefaultAsync(t => t.Id == tripId);
            
            if (trip == null || trip.Bus.OperatorId != operatorId)
                return (false, "Trip not found.", null);

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

            return (true, "Success", new { Passengers = passengers, TotalRevenue = totalRevenue });
        }

        public async Task<object> GetRevenueStatsAsync(int operatorId)
        {
            var myTrips = await _context.Trips.Where(t => t.Bus.OperatorId == operatorId).Select(t => t.Id).ToListAsync();

            var totalRevenue = await _context.Bookings
                .Where(b => myTrips.Contains(b.TripId) && b.Status == "Confirmed")
                .SumAsync(b => b.TotalAmount); 
            
            var operatorRevenue = await _context.BookingSeats
                .Include(bs => bs.Booking)
                .ThenInclude(b => b.Trip)
                .Where(bs => myTrips.Contains(bs.Booking.TripId) && bs.Booking.Status == "Confirmed")
                .SumAsync(bs => bs.Booking.Trip.TicketPrice);

            var totalExpenses = await _context.Expenses
                .Where(e => e.OperatorId == operatorId)
                .SumAsync(e => e.Amount);

            return new
            {
                TotalRevenue = operatorRevenue,
                TotalExpenses = totalExpenses,
                NetProfit = operatorRevenue - totalExpenses
            };
        }

        public async Task<(bool Success, string Message)> AddExpenseAsync(int operatorId, CreateExpenseRequest request)
        {
            var trip = await _context.Trips.Include(t => t.Bus).FirstOrDefaultAsync(t => t.Id == request.TripId);
            if (trip == null || trip.Bus.OperatorId != operatorId)
                return (false, "Trip not found.");

            var expense = new Expense
            {
                OperatorId = operatorId,
                TripId = request.TripId,
                Description = request.Description,
                Amount = request.Amount
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return (true, "Expense recorded successfully.");
        }
    }
}
