using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.DataAccess.DTOs.Bookings;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;
using AutoMapper;

namespace TravelTourManagement.Business.Services;

public class PaymentService : IPaymentService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public PaymentService(IBookingRepository bookingRepository, IRepository<Payment, Guid> paymentRepository, IMapper mapper, INotificationService notificationService)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _notificationService = notificationService;
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
            Currency = "INR",
            PaymentMethod = request.PaymentMethod,
            Status = PaymentStatus.Paid,
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

        // Notify Traveler
        await _notificationService.SendNotificationAsync(
            booking.UserId,
            "Payment Successful",
            $"Your payment of {request.Amount:C} for booking {booking.BookingReference} was successful.",
            booking.Id,
            TravelTourManagement.DataAccess.Enums.NotificationType.payment,
            cancellationToken);

        // Notify Packager
        if (booking.Package != null && booking.Package.Packager != null)
        {
            await _notificationService.SendNotificationAsync(
                booking.Package.Packager.UserId,
                "New Booking Payment",
                $"A payment of {request.Amount:C} was received for booking {booking.BookingReference}, Review thier documents to confirm their booking!",
                booking.Id,
                TravelTourManagement.DataAccess.Enums.NotificationType.payment,
                cancellationToken);
        }

        return _mapper.Map<BookingResponse>(booking);
    }
}
