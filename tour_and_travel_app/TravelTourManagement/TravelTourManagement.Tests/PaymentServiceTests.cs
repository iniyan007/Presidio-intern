using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.DTOs.Bookings;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Tests;

[TestFixture]
public class PaymentServiceTests
{
    private Mock<IBookingRepository> _bookingRepoMock;
    private Mock<IRepository<Payment, Guid>> _paymentRepoMock;
    private Mock<IMapper> _mapperMock;
    private Mock<INotificationService> _notificationServiceMock;
    private PaymentService _paymentService;

    [SetUp]
    public void Setup()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
        _paymentRepoMock = new Mock<IRepository<Payment, Guid>>();
        _mapperMock = new Mock<IMapper>();
        _notificationServiceMock = new Mock<INotificationService>();

        _paymentService = new PaymentService(
            _bookingRepoMock.Object,
            _paymentRepoMock.Object,
            _mapperMock.Object,
            _notificationServiceMock.Object
        );
    }

    [Test]
    public async Task ProcessPaymentAsync_BookingNotFound_ThrowsInvalidOperationException()
    {
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        var request = new ProcessPaymentRequest { Amount = 100, PaymentMethod = "Card", TransactionId = "TXN123" };
        Func<Task> act = async () => await _paymentService.ProcessPaymentAsync(Guid.NewGuid(), Guid.NewGuid(), request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Booking not found.");
    }

    [Test]
    public async Task ProcessPaymentAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var booking = new Booking { Id = Guid.NewGuid(), UserId = Guid.NewGuid() }; // User B
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var request = new ProcessPaymentRequest { Amount = 100, PaymentMethod = "Card", TransactionId = "TXN123" };
        Func<Task> act = async () => await _paymentService.ProcessPaymentAsync(Guid.NewGuid(), booking.Id, request); // User A

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You are not authorized to pay for this booking.");
    }

    [Test]
    public async Task ProcessPaymentAsync_InvalidStatus_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking { Id = Guid.NewGuid(), UserId = userId, Status = BookingStatus.Confirmed };
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var request = new ProcessPaymentRequest { Amount = 100, PaymentMethod = "Card", TransactionId = "TXN123" };
        Func<Task> act = async () => await _paymentService.ProcessPaymentAsync(userId, booking.Id, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Booking is not in a payable state*");
    }

    [Test]
    public async Task ProcessPaymentAsync_ExpiredPaymentWindow_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking 
        { 
            Id = Guid.NewGuid(), 
            UserId = userId, 
            Status = BookingStatus.Pending,
            BookedAt = DateTime.UtcNow.AddMinutes(-6) // More than 5 minutes ago
        };
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var request = new ProcessPaymentRequest { Amount = 100, PaymentMethod = "Card", TransactionId = "TXN123" };
        Func<Task> act = async () => await _paymentService.ProcessPaymentAsync(userId, booking.Id, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("The payment window of 5 minutes has expired. Please create a new booking.");
    }

    [Test]
    public async Task ProcessPaymentAsync_AmountMismatch_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking 
        { 
            Id = Guid.NewGuid(), 
            UserId = userId, 
            Status = BookingStatus.Pending,
            BookedAt = DateTime.UtcNow,
            TotalAmount = 500
        };
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var request = new ProcessPaymentRequest { Amount = 100, PaymentMethod = "Card", TransactionId = "TXN123" }; // Amount mismatch
        Func<Task> act = async () => await _paymentService.ProcessPaymentAsync(userId, booking.Id, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Payment amount must exactly match the total amount of 500");
    }

    [Test]
    public async Task ProcessPaymentAsync_ValidRequest_ProcessesPaymentAndReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking 
        { 
            Id = Guid.NewGuid(), 
            UserId = userId, 
            Status = BookingStatus.Pending,
            BookedAt = DateTime.UtcNow,
            TotalAmount = 500,
            BookingReference = "REF123",
            Package = new Package { Packager = new Packager { UserId = Guid.NewGuid() } }
        };
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var expectedResponse = new BookingResponse(booking.Id, userId, booking.PackageId, booking.BookingReference, 1, 0, 0, 500, 500, "Paid", "Confirmed", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5), null, DateTime.UtcNow, null, null, new System.Collections.Generic.List<BookingTravelerResponse>());
        _mapperMock.Setup(x => x.Map<BookingResponse>(booking)).Returns(expectedResponse);

        var request = new ProcessPaymentRequest { Amount = 500, PaymentMethod = "Card", TransactionId = "TXN123" };
        
        var result = await _paymentService.ProcessPaymentAsync(userId, booking.Id, request);

        // Verify Payment Added
        _paymentRepoMock.Verify(x => x.AddAsync(It.Is<Payment>(p => p.Amount == 500 && p.TransactionId == "TXN123" && p.Status == PaymentStatus.Paid), It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify Booking Updated
        booking.Status.Should().Be(BookingStatus.DocumentUnderReview);
        booking.PaymentStatus.Should().Be(PaymentStatus.Paid);
        _bookingRepoMock.Verify(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()), Times.Once);

        // Verify Notifications
        _notificationServiceMock.Verify(x => x.SendNotificationAsync(userId, It.IsAny<string>(), It.IsAny<string>(), booking.Id, NotificationType.payment, It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(x => x.SendNotificationAsync(booking.Package.Packager.UserId, It.IsAny<string>(), It.IsAny<string>(), booking.Id, NotificationType.payment, It.IsAny<CancellationToken>()), Times.Once);

        result.Should().BeEquivalentTo(expectedResponse);
    }
}
