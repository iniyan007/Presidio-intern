using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.DTOs.Reviews;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Tests;

[TestFixture]
public class ReviewServiceTests
{
    private Mock<IReviewRepository> _reviewRepoMock;
    private Mock<IBookingRepository> _bookingRepoMock;
    private Mock<IPackageRepository> _packageRepoMock;
    private Mock<IPackagerRepository> _packagerRepoMock;
    private Mock<IMapper> _mapperMock;
    private Mock<INotificationService> _notificationServiceMock;
    private ReviewService _reviewService;

    [SetUp]
    public void Setup()
    {
        _reviewRepoMock = new Mock<IReviewRepository>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _packageRepoMock = new Mock<IPackageRepository>();
        _packagerRepoMock = new Mock<IPackagerRepository>();
        _mapperMock = new Mock<IMapper>();
        _notificationServiceMock = new Mock<INotificationService>();

        _reviewService = new ReviewService(
            _reviewRepoMock.Object,
            _bookingRepoMock.Object,
            _packageRepoMock.Object,
            _packagerRepoMock.Object,
            _mapperMock.Object,
            _notificationServiceMock.Object
        );
    }

    [Test]
    public async Task CreateReviewAsync_BookingNotFound_ThrowsKeyNotFoundException()
    {
        _bookingRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        var request = new CreateReviewRequest(Guid.NewGuid(), 5, null, null, null, null, null, null, new List<string>());
        Func<Task> act = async () => await _reviewService.CreateReviewAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Booking not found.");
    }

    [Test]
    public async Task CreateReviewAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var booking = new Booking { Id = Guid.NewGuid(), UserId = Guid.NewGuid() }; // User B
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var request = new CreateReviewRequest(booking.Id, 5, null, null, null, null, null, null, new List<string>());
        Func<Task> act = async () => await _reviewService.CreateReviewAsync(Guid.NewGuid(), request); // User A

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You can only review your own bookings.");
    }

    [Test]
    public async Task CreateReviewAsync_InvalidStatus_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking { Id = Guid.NewGuid(), UserId = userId, Status = BookingStatus.Pending }; // Not Confirmed
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var request = new CreateReviewRequest(booking.Id, 5, null, null, null, null, null, null, new List<string>());
        Func<Task> act = async () => await _reviewService.CreateReviewAsync(userId, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You can only review a booking that is Confirmed or Completed.");
    }

    [Test]
    public async Task CreateReviewAsync_DuplicateReview_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking { Id = Guid.NewGuid(), UserId = userId, Status = BookingStatus.Completed };
        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        
        _reviewRepoMock.Setup(x => x.HasUserReviewedBookingAsync(userId, booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Already reviewed

        var request = new CreateReviewRequest(booking.Id, 5, null, null, null, null, null, null, new List<string>());
        Func<Task> act = async () => await _reviewService.CreateReviewAsync(userId, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You have already submitted a review for this booking.");
    }

    [Test]
    public async Task CreateReviewAsync_ValidRequest_CalculatesAveragesAndSaves()
    {
        var userId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var packagerId = Guid.NewGuid();
        var booking = new Booking 
        { 
            Id = Guid.NewGuid(), 
            UserId = userId, 
            Status = BookingStatus.Completed, 
            PackageId = packageId,
            Package = new Package { PackagerId = packagerId }
        };
        
        var package = new Package { Id = packageId, AvgRating = 4, TotalReviews = 1 };
        var packager = new Packager { Id = packagerId, AvgRating = 3, TotalReviews = 2, UserId = Guid.NewGuid() };

        _bookingRepoMock.Setup(x => x.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _reviewRepoMock.Setup(x => x.HasUserReviewedBookingAsync(userId, booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _packageRepoMock.Setup(x => x.GetByIdAsync(packageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);
        _packagerRepoMock.Setup(x => x.GetByIdAsync(packagerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(packager);

        var expectedResponse = new ReviewResponse(Guid.NewGuid(), booking.Id, userId, "Name", 5, null, null, null, null, null, "Great!", true, null, DateTime.UtcNow, new List<ReviewMediaResponse>());
        _mapperMock.Setup(x => x.Map<ReviewResponse>(It.IsAny<Review>())).Returns(expectedResponse);

        var request = new CreateReviewRequest(booking.Id, 5, null, null, null, null, null, "Great!", new List<string>());
        
        var result = await _reviewService.CreateReviewAsync(userId, request);

        // Verify Review saved
        _reviewRepoMock.Verify(x => x.AddAsync(It.Is<Review>(r => r.OverallRating == 5 && r.Comment == "Great!"), It.IsAny<CancellationToken>()), Times.Once);

        // Verify Package Rating Updated (New Total: 2, New Avg: (4*1 + 5) / 2 = 4.5)
        package.TotalReviews.Should().Be(2);
        package.AvgRating.Should().Be(4.5m);
        _packageRepoMock.Verify(x => x.UpdateAsync(package, It.IsAny<CancellationToken>()), Times.Once);

        // Verify Packager Rating Updated (New Total: 3, New Avg: (3*2 + 5) / 3 = 11/3 = 3.66...)
        packager.TotalReviews.Should().Be(3);
        Math.Round(packager.AvgRating, 2).Should().Be(3.67m);
        _packagerRepoMock.Verify(x => x.UpdateAsync(packager, It.IsAny<CancellationToken>()), Times.Once);

        // Verify Notification Sent
        _notificationServiceMock.Verify(x => x.SendNotificationAsync(packager.UserId, "New Review Received", It.IsAny<string>(), It.IsAny<Guid>(), NotificationType.review, It.IsAny<CancellationToken>()), Times.Once);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public async Task GetReviewsByPackageIdAsync_ReturnsMappedList()
    {
        var packageId = Guid.NewGuid();
        var reviews = new List<Review> { new Review() };
        var responses = new List<ReviewResponse> { new ReviewResponse(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "", 5, null, null, null, null, null, null, true, null, DateTime.UtcNow, new List<ReviewMediaResponse>()) };

        _reviewRepoMock.Setup(x => x.GetByPackageIdAsync(packageId, It.IsAny<CancellationToken>())).ReturnsAsync(reviews);
        _mapperMock.Setup(x => x.Map<IReadOnlyList<ReviewResponse>>(reviews)).Returns(responses);

        var result = await _reviewService.GetReviewsByPackageIdAsync(packageId);

        result.Should().BeEquivalentTo(responses);
    }

    [Test]
    public async Task GetReviewsByPackagerIdAsync_ReturnsMappedList()
    {
        var packagerId = Guid.NewGuid();
        var reviews = new List<Review> { new Review() };
        var responses = new List<ReviewResponse> { new ReviewResponse(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "", 5, null, null, null, null, null, null, true, null, DateTime.UtcNow, new List<ReviewMediaResponse>()) };

        _reviewRepoMock.Setup(x => x.GetByPackagerIdAsync(packagerId, It.IsAny<CancellationToken>())).ReturnsAsync(reviews);
        _mapperMock.Setup(x => x.Map<IReadOnlyList<ReviewResponse>>(reviews)).Returns(responses);

        var result = await _reviewService.GetReviewsByPackagerIdAsync(packagerId);

        result.Should().BeEquivalentTo(responses);
    }
}
