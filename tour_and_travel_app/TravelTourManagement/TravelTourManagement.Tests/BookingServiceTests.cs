using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Quartz;
using TravelTourManagement.DataAccess.DTOs.Bookings;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Tests;

[TestFixture]
public class BookingServiceTests
{
    private Mock<IBookingRepository> _bookingRepoMock;
    private Mock<IPackageRepository> _packageRepoMock;
    private Mock<IRepository<PackageSeasonalPricing, Guid>> _pricingRepoMock;
    private Mock<IUserRepository> _userRepoMock;
    private Mock<IRepository<TravelDocument, Guid>> _documentRepoMock;
    private Mock<ISchedulerFactory> _schedulerFactoryMock;
    private Mock<IPdfService> _pdfServiceMock;
    private Mock<INotificationService> _notificationServiceMock;
    private Mock<IEmailService> _emailServiceMock;
    private Mock<IPlatformConfigService> _platformConfigServiceMock;
    private Mock<IMapper> _mapperMock;
    
    private ApplicationDbContext _dbContext;
    private BookingService _bookingService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _bookingRepoMock = new Mock<IBookingRepository>();
        _packageRepoMock = new Mock<IPackageRepository>();
        _pricingRepoMock = new Mock<IRepository<PackageSeasonalPricing, Guid>>();
        _userRepoMock = new Mock<IUserRepository>();
        _documentRepoMock = new Mock<IRepository<TravelDocument, Guid>>();
        _schedulerFactoryMock = new Mock<ISchedulerFactory>();
        _pdfServiceMock = new Mock<IPdfService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _emailServiceMock = new Mock<IEmailService>();
        _platformConfigServiceMock = new Mock<IPlatformConfigService>();
        _mapperMock = new Mock<IMapper>();

        _platformConfigServiceMock.Setup(x => x.GetConfigAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new TravelTourManagement.DataAccess.DTOs.PlatformConfig.PlatformConfigResponse { PlatformFeePercent = 5.0m, GstPercent = 10.0m });
        
        var schedulerMock = new Mock<IScheduler>();
        _schedulerFactoryMock.Setup(x => x.GetScheduler(It.IsAny<CancellationToken>())).ReturnsAsync(schedulerMock.Object);

        _mapperMock.Setup(m => m.Map<BookingResponse>(It.IsAny<Booking>())).Returns((Booking b) => new BookingResponse(
            b.Id, b.UserId, b.PackageId, b.BookingReference, b.AdultCount, b.ChildCount, b.InfantCount,
            b.TotalAmount, b.PaidAmount, b.PaymentStatus.ToString(), b.TravelDate, b.ReturnDate,
            b.SpecialRequests, b.BookedAt, b.CancelledAt, b.CancellationReason, new List<BookingTravelerResponse>()
        ));

        _bookingService = new BookingService(
            _platformConfigServiceMock.Object,
            _mapperMock.Object,
            _bookingRepoMock.Object,
            _packageRepoMock.Object,
            _pricingRepoMock.Object,
            _userRepoMock.Object,
            _documentRepoMock.Object,
            _schedulerFactoryMock.Object,
            _pdfServiceMock.Object,
            _notificationServiceMock.Object,
            _emailServiceMock.Object,
            _dbContext
        );
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    // --- CreateBookingAsync Tests ---

    [Test]
    public async Task CreateBookingAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var request = new CreateBookingRequest { PackageId = Guid.NewGuid(), SeasonalPricingId = Guid.NewGuid() };

        Func<Task> act = async () => await _bookingService.CreateBookingAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User not found.");
    }

    [Test]
    public async Task CreateBookingAsync_PackageNotPublished_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = userId });
        
        var package = new Package { Id = packageId, Status = PackageStatus.Draft };
        _packageRepoMock.Setup(x => x.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);

        var request = new CreateBookingRequest { PackageId = packageId, SeasonalPricingId = Guid.NewGuid() };
        Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Package is not available for booking.");
    }

    [Test]
    public async Task CreateBookingAsync_PricingNotActive_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();

        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = userId });
        _packageRepoMock.Setup(x => x.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(new Package { Id = packageId, Status = PackageStatus.Published });
        
        var pricing = new PackageSeasonalPricing { Id = pricingId, PackageId = packageId, IsActive = false };
        _pricingRepoMock.Setup(x => x.GetByIdAsync(pricingId, It.IsAny<CancellationToken>())).ReturnsAsync(pricing);

        var request = new CreateBookingRequest { PackageId = packageId, SeasonalPricingId = pricingId };
        Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Pricing tier is not valid or active.");
    }

    [Test]
    public async Task CreateBookingAsync_AdvanceBookingRuleViolated_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();

        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = userId });
        _packageRepoMock.Setup(x => x.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(new Package { Id = packageId, Status = PackageStatus.Published, Type = PackageType.Honeymoon });
        
        var pricing = new PackageSeasonalPricing { Id = pricingId, PackageId = packageId, IsActive = true, StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)) };
        _pricingRepoMock.Setup(x => x.GetByIdAsync(pricingId, It.IsAny<CancellationToken>())).ReturnsAsync(pricing);

        // Travel date is only 15 days away, but rule requires 1 month for Honeymoon
        var request = new CreateBookingRequest { PackageId = packageId, SeasonalPricingId = pricingId, TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)) };
        Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, request);

        await act.Should().ThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>().WithMessage("*packages must be booked at least 1 month in advance.");
    }

    [Test]
    public async Task CreateBookingAsync_OutsideIndiaMissingPassport_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();

        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = userId });
        _packageRepoMock.Setup(x => x.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(new Package { Id = packageId, Status = PackageStatus.Published, Type = PackageType.Group, Country = "USA" });
        
        var pricing = new PackageSeasonalPricing { Id = pricingId, PackageId = packageId, IsActive = true, StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)) };
        _pricingRepoMock.Setup(x => x.GetByIdAsync(pricingId, It.IsAny<CancellationToken>())).ReturnsAsync(pricing);

        var request = new CreateBookingRequest 
        { 
            PackageId = packageId, 
            SeasonalPricingId = pricingId, 
            TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
            Travelers = new List<BookingTravelerRequest> { new BookingTravelerRequest { FullName = "Bob", Age = 30, PassportNumber = null } }
        };
        Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Passport Number is required for all travelers when traveling outside of India.");
    }

    [Test]
    public async Task CreateBookingAsync_MaxCapacityExceeded_ThrowsInvalidOperationException()
    {
        var packageId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        
        var package = new Package { Id = packageId, MaxCapacity = 10, CurrentBookings = 10, Status = PackageStatus.Published, Type = PackageType.Group, Country = "India" };
        var pricing = new PackageSeasonalPricing { Id = pricingId, PackageId = packageId, BasePrice = 1000, AvailableSlots = 5, IsActive = true, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)) };

        var userId = Guid.NewGuid();
        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = userId, FullName = "Test User" });
        _packageRepoMock.Setup(x => x.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        _pricingRepoMock.Setup(x => x.GetByIdAsync(pricingId, It.IsAny<CancellationToken>())).ReturnsAsync(pricing);

        var request = new CreateBookingRequest { PackageId = packageId, SeasonalPricingId = pricingId, TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), InfantCount = 0, Travelers = new List<BookingTravelerRequest> { new BookingTravelerRequest { FullName = "Test", Age = 30 } } };
        Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, request);

        // Current bookings (10) + new seats (1) > MaxCapacity (10)
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Package max capacity exceeded.");
    }

    [Test]
    public async Task CreateBookingAsync_PackageFullyBooked_ThrowsNotEnoughSlotsAvailable()
    {
        var packageId = Guid.NewGuid();
        var pricingId = Guid.NewGuid();
        
        var request = new CreateBookingRequest { PackageId = packageId, SeasonalPricingId = pricingId, TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), InfantCount = 0, Travelers = new List<BookingTravelerRequest> { new BookingTravelerRequest { FullName = "Test", Age = 30 } } };

        var package = new Package { Id = packageId, MaxCapacity = 100, CurrentBookings = 10, Status = PackageStatus.Published, Type = PackageType.Group, Country = "India" };
        var pricing = new PackageSeasonalPricing { Id = pricingId, PackageId = packageId, BasePrice = 1000, AvailableSlots = 0, IsActive = true, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)) };

        var userId = Guid.NewGuid();
        _userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = userId, FullName = "Test User" });
        _packageRepoMock.Setup(x => x.GetByIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        _pricingRepoMock.Setup(x => x.GetByIdAsync(pricingId, It.IsAny<CancellationToken>())).ReturnsAsync(pricing);

        Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Not enough slots available*");
    }

    // --- DownloadTicketAsync Tests ---

    [Test]
    public async Task DownloadTicketAsync_BookingNotFound_ThrowsKeyNotFoundException()
    {
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Booking?)null);
        Func<Task> act = async () => await _bookingService.DownloadBookingTicketAsync(Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Booking not found.");
    }

    [Test]
    public async Task DownloadTicketAsync_UserNotOwner_ThrowsUnauthorizedAccessException()
    {
        var booking = new Booking { Id = Guid.NewGuid(), UserId = Guid.NewGuid() }; // User B
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        
        Func<Task> act = async () => await _bookingService.DownloadBookingTicketAsync(Guid.NewGuid(), booking.Id); // User A requests
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not own this booking.");
    }

    [Test]
    public async Task DownloadTicketAsync_BookingNotConfirmed_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking { Id = Guid.NewGuid(), UserId = userId, Status = BookingStatus.Pending };
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        
        Func<Task> act = async () => await _bookingService.DownloadBookingTicketAsync(userId, booking.Id);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Ticket can only be downloaded for confirmed bookings.");
    }

    // --- CancelBookingAsync Tests ---

    [Test]
    public async Task CancelBookingAsync_BookingNotFound_ThrowsKeyNotFoundException()
    {
        _bookingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Booking?)null);
        Func<Task> act = async () => await _bookingService.CancelBookingAsync(Guid.NewGuid(), Guid.NewGuid(), new CancelBookingRequest { CancellationReason = "Reason" });
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Booking not found.");
    }

    [Test]
    public async Task CancelBookingAsync_UserNotOwner_ThrowsUnauthorizedAccessException()
    {
        var booking = new Booking { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _bookingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        
        Func<Task> act = async () => await _bookingService.CancelBookingAsync(Guid.NewGuid(), booking.Id, new CancelBookingRequest { CancellationReason = "Reason" });
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not own this booking.");
    }

    [Test]
    public async Task CancelBookingAsync_InvalidStatus_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking { Id = Guid.NewGuid(), UserId = userId, Status = BookingStatus.Cancelled };
        _bookingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        
        Func<Task> act = async () => await _bookingService.CancelBookingAsync(userId, booking.Id, new CancelBookingRequest { CancellationReason = "Reason" });
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Booking cannot be cancelled because its current status is Cancelled.");
    }

    // --- ConfirmBookingAsync Tests ---

    [Test]
    public async Task ConfirmBookingAsync_BookingNotFound_ThrowsInvalidOperationException()
    {
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Booking?)null);
        Func<Task> act = async () => await _bookingService.VerifyBookingAsync(Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Booking not found.");
    }

    [Test]
    public async Task ConfirmBookingAsync_PackagerUnauthorized_ThrowsUnauthorizedAccessException()
    {
        var packagerUserId = Guid.NewGuid();
        var booking = new Booking { Id = Guid.NewGuid(), Package = new Package { Packager = new Packager { UserId = Guid.NewGuid() } } };
        
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        
        Func<Task> act = async () => await _bookingService.VerifyBookingAsync(packagerUserId, booking.Id);
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You are not authorized to verify this booking because you don't own the package.");
    }

    [Test]
    public async Task ConfirmBookingAsync_NotInReviewState_ThrowsInvalidOperationException()
    {
        var packagerUserId = Guid.NewGuid();
        var booking = new Booking { Id = Guid.NewGuid(), Status = BookingStatus.Confirmed, Package = new Package { Packager = new Packager { UserId = packagerUserId } } };
        
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        
        Func<Task> act = async () => await _bookingService.VerifyBookingAsync(packagerUserId, booking.Id);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Booking is not in DocumentUnderReview state*");
    }

    [Test]
    public async Task ConfirmBookingAsync_UnverifiedDocuments_ThrowsInvalidOperationException()
    {
        var packagerUserId = Guid.NewGuid();
        var booking = new Booking 
        { 
            Id = Guid.NewGuid(), 
            Status = BookingStatus.DocumentUnderReview, 
            Package = new Package { Packager = new Packager { UserId = packagerUserId } },
            TravelDocuments = new List<TravelDocument> 
            { 
                new TravelDocument { Status = DocumentStatus.Uploaded } 
            } 
        };
        
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        
        Func<Task> act = async () => await _bookingService.VerifyBookingAsync(packagerUserId, booking.Id);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot confirm booking until all travel documents are verified.");
    }

    // --- VerifyDocumentAsync Tests ---

    [Test]
    public async Task VerifyDocumentAsync_DocumentNotFound_ThrowsKeyNotFoundException()
    {
        _documentRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TravelDocument?)null);
        Func<Task> act = async () => await _bookingService.VerifyDocumentAsync(Guid.NewGuid(), Guid.NewGuid(), new VerifyDocumentRequest { IsVerified = true });
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Document not found.");
    }

    [Test]
    public async Task VerifyDocumentAsync_BookingNotFound_ThrowsKeyNotFoundException()
    {
        var docId = Guid.NewGuid();
        var doc = new TravelDocument { Id = docId, TravelerId = Guid.NewGuid(), Traveler = new BookingTraveler { BookingId = Guid.NewGuid() } };
        _documentRepoMock.Setup(x => x.GetByIdAsync(docId, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        _bookingRepoMock.Setup(x => x.GetWithFullDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Booking?)null);

        Func<Task> act = async () => await _bookingService.VerifyDocumentAsync(Guid.NewGuid(), docId, new VerifyDocumentRequest { IsVerified = true });
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Associated booking not found.");
    }

    [Test]
    public async Task VerifyDocumentAsync_PackageNotFound_ThrowsKeyNotFoundException()
    {
        var docId = Guid.NewGuid();
        var booking = new Booking { Id = Guid.NewGuid(), PackageId = Guid.NewGuid() };
        var doc = new TravelDocument { Id = docId, BookingId = booking.Id, Traveler = new BookingTraveler { BookingId = Guid.NewGuid() } };
        _packageRepoMock.Setup(x => x.GetWithFullDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Package?)null);
        
        _documentRepoMock.Setup(x => x.GetByIdAsync(docId, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        _bookingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        
        Func<Task> act = async () => await _bookingService.VerifyDocumentAsync(Guid.NewGuid(), docId, new VerifyDocumentRequest { IsVerified = true });
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Associated package not found.");
    }

    [Test]
    public async Task VerifyDocumentAsync_PackagerUnauthorized_ThrowsUnauthorizedAccessException()
    {
        var packagerUserId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var booking = new Booking { Id = Guid.NewGuid(), PackageId = Guid.NewGuid() };
        var doc = new TravelDocument { Id = docId, BookingId = booking.Id, Traveler = new BookingTraveler { BookingId = Guid.NewGuid() } };
        var package = new Package { Packager = new Packager { UserId = Guid.NewGuid() } };
        _packageRepoMock.Setup(x => x.GetWithFullDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(package);
        
        _documentRepoMock.Setup(x => x.GetByIdAsync(docId, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        _bookingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        
        Func<Task> act = async () => await _bookingService.VerifyDocumentAsync(packagerUserId, docId, new VerifyDocumentRequest { IsVerified = true });
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You are not authorized to verify documents for this package.");
    }
}
