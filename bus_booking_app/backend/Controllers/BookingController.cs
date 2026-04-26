using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using backend.Data;
using backend.Models;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public BookingController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        [HttpPost("lock")]
        public async Task<IActionResult> LockSeats(LockSeatsRequest request)
        {
            var userId = GetUserId();
            var trip = await _context.Trips.FindAsync(request.TripId);
            if (trip == null) return NotFound(new { message = "Trip not found." });

            // Check if seats are already booked or locked
            var existingLocks = await _context.BookingSeats
                .Include(bs => bs.Booking)
                .Where(bs => bs.Booking.TripId == request.TripId && 
                             (bs.Booking.Status == "Confirmed" || 
                              (bs.Booking.Status == "Pending" && bs.Booking.LockedUntil > DateTime.UtcNow)))
                .Select(bs => bs.SeatNumber)
                .ToListAsync();

            var alreadyTaken = request.SeatNumbers.Intersect(existingLocks).ToList();
            if (alreadyTaken.Any())
            {
                return BadRequest(new { message = "Some seats are already taken.", seats = alreadyTaken });
            }

            // Create a pending booking with a 7-minute lock
            var booking = new Booking
            {
                TripId = request.TripId,
                UserId = userId,
                TotalAmount = trip.TotalPrice * request.SeatNumbers.Count,
                Status = "Pending",
                LockedUntil = DateTime.UtcNow.AddMinutes(7)
            };

            foreach (var seatNum in request.SeatNumbers)
            {
                booking.BookingSeats.Add(new BookingSeat { SeatNumber = seatNum });
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Seats locked for 7 minutes.", 
                bookingId = booking.Id, 
                lockedUntil = booking.LockedUntil 
            });
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmBooking(int id, [FromBody] ConfirmBookingRequest request)
        {
            var userId = GetUserId();
            var booking = await _context.Bookings
                .Include(b => b.BookingSeats)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null) return NotFound();
            if (booking.Status != "Pending") return BadRequest(new { message = "Booking is not in pending state." });
            if (booking.LockedUntil < DateTime.UtcNow) return BadRequest(new { message = "Lock expired. Please try again." });

            // Update passenger details
            foreach (var p in request.Passengers)
            {
                var seat = booking.BookingSeats.FirstOrDefault(s => s.SeatNumber == p.SeatNumber);
                if (seat != null)
                {
                    seat.PassengerName = p.Name;
                    seat.PassengerAge = p.Age;
                    seat.PassengerGender = p.Gender;
                }
            }

            booking.Status = "Confirmed";
            await _context.SaveChangesAsync();

            // Fetch full booking details for email (including trip, route, bus, and user)
            var fullBooking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Trip).ThenInclude(t => t.Route)
                .Include(b => b.Trip).ThenInclude(t => t.Bus)
                .Include(b => b.BookingSeats)
                .FirstOrDefaultAsync(b => b.Id == booking.Id);

            if (fullBooking != null && fullBooking.User != null)
            {
                var seatsHtml = "";
                foreach (var seat in fullBooking.BookingSeats)
                {
                    seatsHtml += $@"
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{seat.SeatNumber}</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{seat.PassengerName}</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{seat.PassengerAge}</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{seat.PassengerGender}</td>
                        </tr>";
                }

                var emailHtml = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
                        <h2 style='color: #2563eb; text-align: center;'>Bus Booking Ticket</h2>
                        <div style='text-align: center; color: #16a34a; font-weight: bold; margin-bottom: 20px;'>CONFIRMED</div>
                        
                        <p>Dear {fullBooking.User.Name},</p>
                        <p>Your bus ticket has been successfully booked. Here are your journey details:</p>
                        
                        <div style='background-color: #f8fafc; padding: 15px; border-radius: 8px; margin-bottom: 20px;'>
                            <p><strong>Journey:</strong> {fullBooking.Trip.Route.Source} to {fullBooking.Trip.Route.Destination}</p>
                            <p><strong>Departure:</strong> {fullBooking.Trip.DepartureTime:dd/MM/yyyy HH:mm}</p>
                            <p><strong>Bus:</strong> {fullBooking.Trip.Bus.Name} ({fullBooking.Trip.Bus.BusNumber})</p>
                            <p><strong>Total Amount Paid:</strong> Rs. {fullBooking.TotalAmount}</p>
                        </div>
                        
                        <h3>Passenger Details</h3>
                        <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                            <thead>
                                <tr style='background-color: #2563eb; color: white;'>
                                    <th style='padding: 8px; border: 1px solid #ddd;'>Seat No.</th>
                                    <th style='padding: 8px; border: 1px solid #ddd;'>Name</th>
                                    <th style='padding: 8px; border: 1px solid #ddd;'>Age</th>
                                    <th style='padding: 8px; border: 1px solid #ddd;'>Gender</th>
                                </tr>
                            </thead>
                            <tbody>
                                {seatsHtml}
                            </tbody>
                        </table>
                        
                        <p style='text-align: center; color: #64748b; font-size: 12px; margin-top: 30px;'>
                            Please carry a digital or printed copy of this email and a valid ID proof during the journey.<br>
                            Happy Journey!
                        </p>
                    </div>";

                await _emailService.SendEmailAsync(fullBooking.User.Email, "Your Bus Ticket Confirmation", emailHtml, true);
            }

            return Ok(new { message = "Booking confirmed successfully!", bookingId = booking.Id });
        }

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = GetUserId();
            var bookings = await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Route)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Bus)
                .Include(b => b.BookingSeats)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new {
                    b.Id,
                    b.TotalAmount,
                    b.Status,
                    b.CreatedAt,
                    Trip = new {
                        b.Trip.DepartureTime,
                        Route = new {
                            b.Trip.Route.Source,
                            b.Trip.Route.Destination
                        },
                        Bus = new {
                            b.Trip.Bus.Name,
                            b.Trip.Bus.BusNumber
                        }
                    },
                    Seats = b.BookingSeats.Select(s => new {
                        s.SeatNumber,
                        s.PassengerName,
                        s.PassengerAge,
                        s.PassengerGender
                    }).ToList()
                })
                .ToListAsync();

            return Ok(bookings);
        }
    }

    public class LockSeatsRequest
    {
        public int TripId { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
    }

    public class ConfirmBookingRequest
    {
        public List<PassengerDetail> Passengers { get; set; } = new();
    }

    public class PassengerDetail
    {
        public string SeatNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Gender { get; set; } = "";
    }
}
