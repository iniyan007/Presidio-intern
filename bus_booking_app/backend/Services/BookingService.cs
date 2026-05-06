using backend.Interfaces;
using backend.Data;
using backend.Models;
using backend.Controllers;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public BookingService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<(bool Success, string Message, int? BookingId, DateTime? LockedUntil, List<string>? ConflictingSeats)> LockSeatsAsync(int userId, int tripId, List<string> seatNumbers)
        {
            var trip = await _context.Trips.FindAsync(tripId);
            if (trip == null) return (false, "Trip not found.", null, null, null);

            var existingLocks = await _context.BookingSeats
                .Include(bs => bs.Booking)
                .Where(bs => bs.Booking.TripId == tripId && 
                             (bs.Booking.Status == "Confirmed" || 
                              (bs.Booking.Status == "Pending" && bs.Booking.LockedUntil > DateTime.UtcNow)))
                .Select(bs => bs.SeatNumber)
                .ToListAsync();

            var alreadyTaken = seatNumbers.Intersect(existingLocks).ToList();
            if (alreadyTaken.Any())
            {
                return (false, "Some seats are already taken.", null, null, alreadyTaken);
            }

            var booking = new Booking
            {
                TripId = tripId,
                UserId = userId,
                TotalAmount = trip.TotalPrice * seatNumbers.Count,
                Status = "Pending",
                LockedUntil = DateTime.UtcNow.AddMinutes(7)
            };

            foreach (var seatNum in seatNumbers)
            {
                booking.BookingSeats.Add(new BookingSeat { SeatNumber = seatNum });
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return (true, "Seats locked for 7 minutes.", booking.Id, booking.LockedUntil, null);
        }

        public async Task<(bool Success, string Message, int? BookingId)> ConfirmBookingAsync(int userId, int bookingId, List<PassengerDetail> passengers)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingSeats)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null) return (false, "Booking not found.", null);
            if (booking.Status != "Pending") return (false, "Booking is not in pending state.", null);
            if (booking.LockedUntil < DateTime.UtcNow) return (false, "Lock expired. Please try again.", null);

            foreach (var p in passengers)
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

            return (true, "Booking confirmed successfully!", booking.Id);
        }

        public async Task<List<BookingHistoryDto>> GetMyBookingsAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Route)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Bus)
                .Include(b => b.BookingSeats)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingHistoryDto {
                    Id = b.Id,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    Trip = new BookingTripDto {
                        DepartureTime = b.Trip.DepartureTime,
                        Route = new RouteDto {
                            Source = b.Trip.Route.Source,
                            Destination = b.Trip.Route.Destination
                        },
                        Bus = new BusDto {
                            Name = b.Trip.Bus.Name,
                            BusNumber = b.Trip.Bus.BusNumber
                        }
                    },
                    Seats = b.BookingSeats.Select(s => new BookingSeatDto {
                        SeatNumber = s.SeatNumber,
                        PassengerName = s.PassengerName,
                        PassengerAge = s.PassengerAge,
                        PassengerGender = s.PassengerGender
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}
