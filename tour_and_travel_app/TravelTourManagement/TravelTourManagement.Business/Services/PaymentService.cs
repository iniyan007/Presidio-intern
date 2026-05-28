using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Bookings;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Business.Services;

public class PaymentService : IPaymentService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;

    public PaymentService(IBookingRepository bookingRepository, IRepository<Payment, Guid> paymentRepository)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<BookingResponse> ProcessPaymentAsync(Guid userId, Guid bookingId, ProcessPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetWithFullDetailsAsync(bookingId, cancellationToken);
        if (booking == null)
            throw new InvalidOperationException("Booking not found.");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to pay for this booking.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Booking is not in a payable state. Current status: {booking.Status}");

        if (DateTime.UtcNow > booking.BookedAt.AddMinutes(5))
            throw new InvalidOperationException("The payment window of 5 minutes has expired. Please create a new booking.");

        if (request.Amount != booking.TotalAmount)
            throw new InvalidOperationException($"Payment amount must exactly match the total amount of {booking.TotalAmount}");

        // Create Payment Record
        var payment = new Payment
        {
            BookingId = booking.Id,
            TransactionId = request.TransactionId,
            Amount = request.Amount,
            Currency = "USD",
            PaymentMethod = request.PaymentMethod,
            GatewayResponse = "Simulated Success",
            PaidAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        // Update Booking
        booking.PaidAmount = request.Amount;
        booking.Status = BookingStatus.DocumentUnderReview;
        booking.PaymentStatus = PaymentStatus.Paid;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        return new BookingResponse(
            booking.Id,
            booking.UserId,
            booking.PackageId,
            booking.BookingReference,
            booking.AdultCount,
            booking.ChildCount,
            booking.InfantCount,
            booking.TotalAmount,
            booking.PaidAmount,
            booking.PaymentStatus.ToString(),
            booking.TravelDate,
            booking.ReturnDate,
            booking.SpecialRequests,
            booking.BookedAt,
            booking.CancelledAt,
            booking.CancellationReason,
            booking.BookingTravelers.Select(t => new BookingTravelerResponse(
                t.Id,
                t.FullName,
                t.PassportNumber,
                t.DateOfBirth,
                t.Nationality,
                t.Age,
                t.Gender,
                t.MealPreference,
                t.AadharCardNumber,
                t.IsPrimary,
                t.TravelDocuments?.Select(d => new TravelDocumentResponse(d.Id, d.DocumentType, d.FilePath, d.FileName, d.UploadedAt)).ToList() ?? new List<TravelDocumentResponse>()
            )).ToList()
        );
    }
}
