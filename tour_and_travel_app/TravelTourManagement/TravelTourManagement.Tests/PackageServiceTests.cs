using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.DTOs;
using TravelTourManagement.DataAccess.DTOs.Packages;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Tests;

[TestFixture]
public class PackageServiceTests
{
    private Mock<IPackageRepository> _packageRepoMock;
    private Mock<IPackagerRepository> _packagerRepoMock;
    private Mock<IBookingRepository> _bookingRepoMock;
    private Mock<IMapper> _mapperMock;
    private PackageService _packageService;

    [SetUp]
    public void Setup()
    {
        _packageRepoMock = new Mock<IPackageRepository>();
        _packagerRepoMock = new Mock<IPackagerRepository>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _mapperMock = new Mock<IMapper>();

        _packageService = new PackageService(
            _packageRepoMock.Object,
            _packagerRepoMock.Object,
            _bookingRepoMock.Object,
            _mapperMock.Object
        );
    }

    // --- CreatePackageAsync Tests ---

    [Test]
    public async Task CreatePackageAsync_PackagerNotFoundOrUnapproved_ThrowsUnauthorizedAccessException()
    {
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Packager?)null);
        var request = new CreatePackageRequest("Test", null, "Dest", "Country", null, 1, 0, 10, null, null, "group", "draft", new List<CreatePackageHighlightRequest>(), new List<CreatePackageInclusionRequest>(), new List<CreatePackageMediaRequest>(), new List<CreatePackagePricingRequest>(), new List<CreateItineraryDayRequest>());

        Func<Task> act = async () => await _packageService.CreatePackageAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Only approved packagers can create packages.");
    }

    [Test]
    public async Task CreatePackageAsync_PackagerApproved_CreatesPackage()
    {
        var packagerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var packager = new Packager { Id = packagerId, ApprovedAt = DateTime.UtcNow, DeactivatedAt = null };
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(packager);

        var request = new CreatePackageRequest("Test Package", null, "Dest", "Country", null, 1, 0, 10, null, null, "group", "draft", new List<CreatePackageHighlightRequest>(), new List<CreatePackageInclusionRequest>(), new List<CreatePackageMediaRequest>(), new List<CreatePackagePricingRequest>(), new List<CreateItineraryDayRequest>());
        var package = new Package { Id = Guid.NewGuid(), Title = "Test Package" };

        _mapperMock.Setup(x => x.Map<Package>(request)).Returns(package);
        _packageRepoMock.Setup(x => x.CreatePackageWithDetailsAsync(It.IsAny<Package>(), It.IsAny<CancellationToken>())).ReturnsAsync(package);

        var result = await _packageService.CreatePackageAsync(userId, request);

        result.Should().Be(package.Id);
        _packageRepoMock.Verify(x => x.CreatePackageWithDetailsAsync(It.IsAny<Package>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- DeletePackageAsync Tests ---

    [Test]
    public async Task DeletePackageAsync_PackagerNotFound_ThrowsUnauthorizedAccessException()
    {
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Packager?)null);

        Func<Task> act = async () => await _packageService.DeletePackageAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Only packagers can delete packages.");
    }

    [Test]
    public async Task DeletePackageAsync_PackageNotFound_ThrowsKeyNotFoundException()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Package?)null);

        Func<Task> act = async () => await _packageService.DeletePackageAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Package not found.");
    }

    [Test]
    public async Task DeletePackageAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        var package = new Package { Id = Guid.NewGuid(), PackagerId = Guid.NewGuid() }; // Different Packager
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(package);

        Func<Task> act = async () => await _packageService.DeletePackageAsync(Guid.NewGuid(), package.Id);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to delete this package.");
    }

    [Test]
    public async Task DeletePackageAsync_ValidRequest_UpdatesStatusToArchived()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        var package = new Package { Id = Guid.NewGuid(), PackagerId = packager.Id, Status = PackageStatus.Published };
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(package);

        await _packageService.DeletePackageAsync(Guid.NewGuid(), package.Id);

        package.Status.Should().Be(PackageStatus.Archived);
        _packageRepoMock.Verify(x => x.UpdateAsync(package, It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- UploadPackageMediaAsync Tests ---

    [Test]
    public async Task UploadPackageMediaAsync_PackagerNotFound_ThrowsUnauthorizedAccessException()
    {
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Packager?)null);

        using var stream = new MemoryStream();
        Func<Task> act = async () => await _packageService.UploadPackageMediaAsync(Guid.NewGuid(), Guid.NewGuid(), stream, "file.jpg", "image/jpeg", "Image", true, 1, "Caption");

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Only packagers can modify packages.");
    }

    [Test]
    public async Task UploadPackageMediaAsync_PackageNotFound_ThrowsKeyNotFoundException()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Package?)null);

        using var stream = new MemoryStream();
        Func<Task> act = async () => await _packageService.UploadPackageMediaAsync(Guid.NewGuid(), Guid.NewGuid(), stream, "file.jpg", "image/jpeg", "Image", true, 1, "Caption");

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Package not found.");
    }

    [Test]
    public async Task UploadPackageMediaAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        var package = new Package { Id = Guid.NewGuid(), PackagerId = Guid.NewGuid() }; // Different Packager
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(package);

        using var stream = new MemoryStream();
        Func<Task> act = async () => await _packageService.UploadPackageMediaAsync(Guid.NewGuid(), package.Id, stream, "file.jpg", "image/jpeg", "Image", true, 1, "Caption");

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to modify this package.");
    }

    // --- GetPublishedPackageByIdAsync Tests ---

    [Test]
    public async Task GetPublishedPackageByIdAsync_PackageNotFound_ThrowsKeyNotFoundException()
    {
        _packageRepoMock.Setup(x => x.GetWithFullDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Package?)null);

        Func<Task> act = async () => await _packageService.GetPublishedPackageByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Published package not found.");
    }

    [Test]
    public async Task GetPublishedPackageByIdAsync_PackageNotPublished_ThrowsKeyNotFoundException()
    {
        var package = new Package { Id = Guid.NewGuid(), Status = PackageStatus.Draft };
        _packageRepoMock.Setup(x => x.GetWithFullDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(package);

        Func<Task> act = async () => await _packageService.GetPublishedPackageByIdAsync(package.Id);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Published package not found.");
    }

    [Test]
    public async Task GetPublishedPackageByIdAsync_ValidPackage_ReturnsResponse()
    {
        var package = new Package { Id = Guid.NewGuid(), Status = PackageStatus.Published };
        var response = new PackageDetailResponse(package.Id, Guid.NewGuid(), "Name", "Title", "Type", null, "Dest", "Country", null, 1, 0, 10, 0, null, null, false, 0, 0, new List<string>(), new List<string>(), new List<string>(), new List<PackageMediaDto>(), new List<PackageSeasonalPricingDto>(), new List<ItineraryDayDto>());

        _packageRepoMock.Setup(x => x.GetWithFullDetailsAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        _mapperMock.Setup(x => x.Map<PackageDetailResponse>(package)).Returns(response);

        var result = await _packageService.GetPublishedPackageByIdAsync(package.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(package.Id);
    }

    // --- GetPackageRevenueAsync Tests ---

    [Test]
    public async Task GetPackageRevenueAsync_PackageNotFound_ThrowsKeyNotFoundException()
    {
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Package?)null);

        Func<Task> act = async () => await _packageService.GetPackageRevenueAsync(Guid.NewGuid(), "Admin", Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Package not found.");
    }

    [Test]
    public async Task GetPackageRevenueAsync_PackagerUnauthorized_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        var package = new Package { Id = Guid.NewGuid(), PackagerId = Guid.NewGuid() }; // Different Packager
        var packager = new Packager { Id = Guid.NewGuid() }; // Mismatch
        
        _packageRepoMock.Setup(x => x.GetByIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(packager);

        Func<Task> act = async () => await _packageService.GetPackageRevenueAsync(userId, "Packager", package.Id);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to view revenue for this package.");
    }

    [Test]
    public async Task GetPackageRevenueAsync_Admin_CalculatesPlatformFee()
    {
        var package = new Package { Id = Guid.NewGuid(), Title = "Test Package" };
        _packageRepoMock.Setup(x => x.GetByIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(package);

        var bookings = new List<Booking>
        {
            new Booking { Status = BookingStatus.Confirmed, PlatformFeeAmount = 50, PackagerBaseAmount = 950 },
            new Booking { Status = BookingStatus.Completed, PlatformFeeAmount = 50, PackagerBaseAmount = 950 },
            new Booking { Status = BookingStatus.Cancelled, PlatformFeeAmount = 50, PackagerBaseAmount = 950 } // Ignored
        };
        _bookingRepoMock.Setup(x => x.GetByPackageIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(bookings);

        var result = await _packageService.GetPackageRevenueAsync(Guid.NewGuid(), "Admin", package.Id);

        result.Revenue.Should().Be(100);
        result.RevenueType.Should().Be("Platform Fee");
        result.TotalConfirmedBookings.Should().Be(2);
    }

    [Test]
    public async Task GetPackageRevenueAsync_Packager_CalculatesPackagerEarnings()
    {
        var userId = Guid.NewGuid();
        var packagerId = Guid.NewGuid();
        var package = new Package { Id = Guid.NewGuid(), PackagerId = packagerId, Title = "Test Package" };
        var packager = new Packager { Id = packagerId };

        _packageRepoMock.Setup(x => x.GetByIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(package);
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(packager);

        var bookings = new List<Booking>
        {
            new Booking { Status = BookingStatus.Confirmed, PlatformFeeAmount = 50, PackagerBaseAmount = 950 },
            new Booking { Status = BookingStatus.Completed, PlatformFeeAmount = 50, PackagerBaseAmount = 950 }
        };
        _bookingRepoMock.Setup(x => x.GetByPackageIdAsync(package.Id, It.IsAny<CancellationToken>())).ReturnsAsync(bookings);

        var result = await _packageService.GetPackageRevenueAsync(userId, "Packager", package.Id);

        result.Revenue.Should().Be(1900);
        result.RevenueType.Should().Be("Packager Earnings");
        result.TotalConfirmedBookings.Should().Be(2);
    }

    // --- UpdatePackageDetailsAsync Tests ---

    [Test]
    public async Task UpdatePackageDetailsAsync_PackagerNotFound_ThrowsUnauthorizedAccessException()
    {
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Packager?)null);

        Func<Task> act = async () => await _packageService.UpdatePackageDetailsAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdatePackageDetailsRequest());

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Packager profile not found.");
    }

    [Test]
    public async Task UpdatePackageDetailsAsync_PackageNotFound_ThrowsKeyNotFoundException()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Package?)null);

        Func<Task> act = async () => await _packageService.UpdatePackageDetailsAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdatePackageDetailsRequest());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Package not found.");
    }

    [Test]
    public async Task UpdatePackageDetailsAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        var package = new Package { Id = Guid.NewGuid(), PackagerId = Guid.NewGuid() }; // Different Packager
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(package);

        Func<Task> act = async () => await _packageService.UpdatePackageDetailsAsync(Guid.NewGuid(), package.Id, new UpdatePackageDetailsRequest());

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not own this package.");
    }

    [Test]
    public async Task UpdatePackageDetailsAsync_ValidRequest_UpdatesPackage()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        var package = new Package { Id = Guid.NewGuid(), PackagerId = packager.Id, Title = "Old Title" };
        
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(package);

        var request = new UpdatePackageDetailsRequest { Title = "New Title" };
        await _packageService.UpdatePackageDetailsAsync(Guid.NewGuid(), package.Id, request);

        package.Title.Should().Be("New Title");
        _packageRepoMock.Verify(x => x.UpdateAsync(package, It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- RepublishPackageAsync Tests ---

    [Test]
    public async Task RepublishPackageAsync_PackagerNotFound_ThrowsUnauthorizedAccessException()
    {
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Packager?)null);

        Func<Task> act = async () => await _packageService.RepublishPackageAsync(Guid.NewGuid(), Guid.NewGuid(), new RepublishPackageRequest());

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Packager profile not found.");
    }

    [Test]
    public async Task RepublishPackageAsync_PackageNotFound_ThrowsKeyNotFoundException()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Package?)null);

        Func<Task> act = async () => await _packageService.RepublishPackageAsync(Guid.NewGuid(), Guid.NewGuid(), new RepublishPackageRequest());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Package not found.");
    }

    [Test]
    public async Task RepublishPackageAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        var package = new Package { Id = Guid.NewGuid(), PackagerId = Guid.NewGuid() }; // Different Packager
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(package);

        Func<Task> act = async () => await _packageService.RepublishPackageAsync(Guid.NewGuid(), package.Id, new RepublishPackageRequest());

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not own this package.");
    }

    [Test]
    public async Task RepublishPackageAsync_ValidRequest_AddsPricingAndRepublishes()
    {
        var packager = new Packager { Id = Guid.NewGuid() };
        var package = new Package { Id = Guid.NewGuid(), PackagerId = packager.Id, Status = PackageStatus.Archived, PackageSeasonalPricings = new List<PackageSeasonalPricing>() };
        
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _packageRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(package);

        var request = new RepublishPackageRequest 
        { 
            SeasonalPricing = new List<TravelTourManagement.DataAccess.DTOs.Packages.CreatePackagePricingRequest>
            {
                new TravelTourManagement.DataAccess.DTOs.Packages.CreatePackagePricingRequest("Summer", null, null, 1000, 0, 0, 10, true)
            }
        };

        await _packageService.RepublishPackageAsync(Guid.NewGuid(), package.Id, request);

        package.Status.Should().Be(PackageStatus.Published);
        package.PackageSeasonalPricings.Count.Should().Be(1);
        package.PackageSeasonalPricings.First().SeasonName.Should().Be("Summer");
        _packageRepoMock.Verify(x => x.UpdateAsync(package, It.IsAny<CancellationToken>()), Times.Once);
    }
}
