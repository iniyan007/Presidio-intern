using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Quartz;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Tests;

[TestFixture]
public class BackgroundJobsTests
{
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private Mock<IServiceScope> _serviceScopeMock;
    
    private Mock<IBookingRepository> _bookingRepoMock;
    private Mock<IPackageRepository> _packageRepoMock;
    private Mock<IRepository<PackageSeasonalPricing, Guid>> _pricingRepoMock;

    [SetUp]
    public void Setup()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();

        _bookingRepoMock = new Mock<IBookingRepository>();
        _packageRepoMock = new Mock<IPackageRepository>();
        _pricingRepoMock = new Mock<IRepository<PackageSeasonalPricing, Guid>>();

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);

        _serviceScopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(_serviceScopeMock.Object);

        var scopedServiceProviderMock = new Mock<IServiceProvider>();
        
        scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(IBookingRepository)))
            .Returns(_bookingRepoMock.Object);
            
        scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(IPackageRepository)))
            .Returns(_packageRepoMock.Object);
            
        scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(IRepository<PackageSeasonalPricing, Guid>)))
            .Returns(_pricingRepoMock.Object);

        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(scopedServiceProviderMock.Object);
    }

    [Test]
    public async Task BookingTimeoutJob_BookingPending_CancelsBookingAndReleasesSeats()
    {
        var loggerMock = new Mock<ILogger<BookingTimeoutJob>>();
        var job = new BookingTimeoutJob(_serviceProviderMock.Object, loggerMock.Object);

        var bookingId = Guid.NewGuid();
        var contextMock = new Mock<IJobExecutionContext>();
        var dataMap = new JobDataMap();
        dataMap["BookingId"] = bookingId.ToString();
        contextMock.Setup(x => x.MergedJobDataMap).Returns(dataMap);
        contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var booking = new Booking
        {
            Id = bookingId,
            Status = BookingStatus.Pending,
            AdultCount = 2,
            ChildCount = 1,
            SeasonalPricingId = Guid.NewGuid(),
            PackageId = Guid.NewGuid()
        };

        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        
        var pricing = new PackageSeasonalPricing { Id = booking.SeasonalPricingId, AvailableSlots = 5 };
        _pricingRepoMock.Setup(x => x.GetByIdAsync(booking.SeasonalPricingId, It.IsAny<CancellationToken>())).ReturnsAsync(pricing);

        var package = new Package { Id = booking.PackageId, CurrentBookings = 10 };
        _packageRepoMock.Setup(x => x.GetByIdAsync(booking.PackageId, It.IsAny<CancellationToken>())).ReturnsAsync(package);

        await job.Execute(contextMock.Object);

        booking.Status.Should().Be(BookingStatus.Cancelled);
        _bookingRepoMock.Verify(x => x.UpdateAsync(booking, It.IsAny<CancellationToken>()), Times.Once);

        pricing.AvailableSlots.Should().Be(8); // 5 + 3
        _pricingRepoMock.Verify(x => x.UpdateAsync(pricing, It.IsAny<CancellationToken>()), Times.Once);

        package.CurrentBookings.Should().Be(7); // 10 - 3
        _packageRepoMock.Verify(x => x.UpdateAsync(package, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task BookingTimeoutJob_BookingAlreadyPaid_DoesNothing()
    {
        var loggerMock = new Mock<ILogger<BookingTimeoutJob>>();
        var job = new BookingTimeoutJob(_serviceProviderMock.Object, loggerMock.Object);

        var bookingId = Guid.NewGuid();
        var contextMock = new Mock<IJobExecutionContext>();
        var dataMap = new JobDataMap();
        dataMap["BookingId"] = bookingId.ToString();
        contextMock.Setup(x => x.MergedJobDataMap).Returns(dataMap);

        var booking = new Booking
        {
            Id = bookingId,
            Status = BookingStatus.Confirmed
        };

        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        await job.Execute(contextMock.Object);

        _bookingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task PackageCompletionJob_PackagesPastEndDate_MarksAsCompleted()
    {
        var loggerMock = new Mock<ILogger<PackageCompletionJob>>();
        var job = new PackageCompletionJob(_serviceProviderMock.Object, loggerMock.Object);

        var contextMock = new Mock<IJobExecutionContext>();

        var package1 = new Package { Id = Guid.NewGuid(), Status = PackageStatus.Published };
        var package2 = new Package { Id = Guid.NewGuid(), Status = PackageStatus.Published }; // Won't complete

        var allPackages = new List<Package> { package1, package2 };
        _packageRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(allPackages);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var pastDate = today.AddDays(-1);
        var futureDate = today.AddDays(1);

        var allPricings = new List<PackageSeasonalPricing>
        {
            new PackageSeasonalPricing { PackageId = package1.Id, IsActive = true, EndDate = pastDate },
            new PackageSeasonalPricing { PackageId = package2.Id, IsActive = true, EndDate = futureDate }
        };

        _pricingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(allPricings);

        await job.Execute(contextMock.Object);

        package1.Status.Should().Be(PackageStatus.Completed);
        _packageRepoMock.Verify(x => x.UpdateAsync(package1, It.IsAny<CancellationToken>()), Times.Once);

        package2.Status.Should().Be(PackageStatus.Published); // Should not change
        _packageRepoMock.Verify(x => x.UpdateAsync(package2, It.IsAny<CancellationToken>()), Times.Never);
    }
}
