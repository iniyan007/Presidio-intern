using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using TravelTourManagement.Business.Interface;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.DTOs;
using TravelTourManagement.DataAccess.DTOs.Packagers;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Interface;

namespace TravelTourManagement.Tests;

[TestFixture]
public class PackagerServiceTests
{
    private Mock<IPackagerRepository> _packagerRepoMock;
    private Mock<IUserRepository> _userRepoMock;
    private Mock<IMapper> _mapperMock;
    private Mock<INotificationService> _notificationServiceMock;
    private Mock<IPackageRepository> _packageRepoMock;
    private Mock<IBookingRepository> _bookingRepoMock;
    private Mock<IRepository<PackageSeasonalPricing, Guid>> _seasonalPricingRepoMock;
    private Mock<Microsoft.Extensions.Caching.Distributed.IDistributedCache> _cacheMock;
    private PackagerService _packagerService;

    [SetUp]
    public void Setup()
    {
        _packagerRepoMock = new Mock<IPackagerRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _notificationServiceMock = new Mock<INotificationService>();
        _packageRepoMock = new Mock<IPackageRepository>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _seasonalPricingRepoMock = new Mock<IRepository<PackageSeasonalPricing, Guid>>();
        _cacheMock = new Mock<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();

        _packagerService = new PackagerService(
            _packagerRepoMock.Object,
            _userRepoMock.Object,
            _mapperMock.Object,
            _notificationServiceMock.Object,
            _packageRepoMock.Object,
            _bookingRepoMock.Object,
            _seasonalPricingRepoMock.Object,
            _cacheMock.Object
        );
    }

    [Test]
    public async Task ApplyToBecomePackagerAsync_AlreadyExists_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        _packagerRepoMock.Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var request = new ApplyPackagerRequest { CompanyName = "Company", BusinessLicenseNo = "123", Description = "Desc", ContactEmail = "email", ContactPhone = "phone", WebsiteUrl = "url" };
        Func<Task> act = async () => await _packagerService.ApplyToBecomePackagerAsync(userId, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You have already submitted a packager application or are already a packager.");
    }

    [Test]
    public async Task ApplyToBecomePackagerAsync_ValidRequest_SavesAndReturns()
    {
        var userId = Guid.NewGuid();
        _packagerRepoMock.Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _packagerRepoMock.Setup(x => x.AddAsync(It.IsAny<Packager>(), It.IsAny<CancellationToken>())).ReturnsAsync((Packager p, CancellationToken c) => p);

        var request = new ApplyPackagerRequest { CompanyName = "Company", BusinessLicenseNo = "123", Description = "Desc", ContactEmail = "email", ContactPhone = "phone", WebsiteUrl = "url" };
        var expectedResponse = new PackagerResponse(Guid.NewGuid(), userId, "Company", "123", "Desc", "email", "phone", "url", "pending", null, 0, 0, DateTime.UtcNow, null);
        _mapperMock.Setup(x => x.Map<PackagerResponse>(It.IsAny<Packager>())).Returns(expectedResponse);

        var result = await _packagerService.ApplyToBecomePackagerAsync(userId, request);

        _packagerRepoMock.Verify(x => x.AddAsync(It.Is<Packager>(p => p.CompanyName == "Company"), It.IsAny<CancellationToken>()), Times.Once);
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public async Task ApprovePackagerAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _packagerRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Packager?)null);
        Func<Task> act = async () => await _packagerService.ApprovePackagerAsync(Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Packager application not found.");
    }

    [Test]
    public async Task ApprovePackagerAsync_AlreadyApproved_ThrowsInvalidOperationException()
    {
        var packager = new Packager { Id = Guid.NewGuid(), ApprovedAt = DateTime.UtcNow };
        _packagerRepoMock.Setup(x => x.GetByIdAsync(packager.Id, It.IsAny<CancellationToken>())).ReturnsAsync(packager);

        Func<Task> act = async () => await _packagerService.ApprovePackagerAsync(packager.Id, Guid.NewGuid());
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Packager is already approved.");
    }

    [Test]
    public async Task ApprovePackagerAsync_AdminNotFound_ThrowsUnauthorizedAccessException()
    {
        var packager = new Packager { Id = Guid.NewGuid(), ApprovedAt = null };
        _packagerRepoMock.Setup(x => x.GetByIdAsync(packager.Id, It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        Func<Task> act = async () => await _packagerService.ApprovePackagerAsync(packager.Id, Guid.NewGuid());
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Admin user not found.");
    }

    [Test]
    public async Task ApprovePackagerAsync_ValidRequest_UpdatesAndSendsNotification()
    {
        var adminId = Guid.NewGuid();
        var packager = new Packager { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ApprovedAt = null };
        _packagerRepoMock.Setup(x => x.GetByIdAsync(packager.Id, It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _userRepoMock.Setup(x => x.GetByIdAsync(adminId, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = adminId });

        var expectedResponse = new PackagerResponse(packager.Id, packager.UserId, "Company", "123", "Desc", "email", "phone", "url", "approved", null, 0, 0, DateTime.UtcNow, null);
        _mapperMock.Setup(x => x.Map<PackagerResponse>(It.IsAny<Packager>())).Returns(expectedResponse);

        var result = await _packagerService.ApprovePackagerAsync(packager.Id, adminId);

        packager.ApprovedBy.Should().Be(adminId);
        packager.ApprovedAt.Should().NotBeNull();
        _packagerRepoMock.Verify(x => x.UpdateAsync(packager, It.IsAny<CancellationToken>()), Times.Once);
        _packagerRepoMock.Verify(x => x.UpdateStatusRawAsync(packager.Id, "approved", It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(x => x.SendNotificationAsync(packager.UserId, "Packager Application Approved", It.IsAny<string>(), packager.Id, NotificationType.approval, It.IsAny<CancellationToken>()), Times.Once);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public async Task RejectPackagerAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _packagerRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Packager?)null);
        Func<Task> act = async () => await _packagerService.RejectPackagerAsync(Guid.NewGuid(), Guid.NewGuid(), "reason");
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Packager application not found.");
    }

    [Test]
    public async Task RejectPackagerAsync_AlreadyApproved_ThrowsInvalidOperationException()
    {
        var packager = new Packager { Id = Guid.NewGuid(), ApprovedAt = DateTime.UtcNow };
        _packagerRepoMock.Setup(x => x.GetByIdAsync(packager.Id, It.IsAny<CancellationToken>())).ReturnsAsync(packager);

        Func<Task> act = async () => await _packagerService.RejectPackagerAsync(packager.Id, Guid.NewGuid(), "reason");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot reject an already approved packager. Deactivate them instead.");
    }

    [Test]
    public async Task RejectPackagerAsync_ValidRequest_UpdatesAndSendsNotification()
    {
        var adminId = Guid.NewGuid();
        var packager = new Packager { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ApprovedAt = null };
        _packagerRepoMock.Setup(x => x.GetByIdAsync(packager.Id, It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _userRepoMock.Setup(x => x.GetByIdAsync(adminId, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = adminId });

        var expectedResponse = new PackagerResponse(packager.Id, packager.UserId, "Company", "123", "Desc", "email", "phone", "url", "deactivated", "Not enough experience.", 0, 0, DateTime.UtcNow, DateTime.UtcNow);
        _mapperMock.Setup(x => x.Map<PackagerResponse>(It.IsAny<Packager>())).Returns(expectedResponse);

        var result = await _packagerService.RejectPackagerAsync(packager.Id, adminId, "Not enough experience.");

        packager.DeactivatedAt.Should().NotBeNull();
        packager.DeactivationReason.Should().Be("Not enough experience.");
        _packagerRepoMock.Verify(x => x.UpdateAsync(packager, It.IsAny<CancellationToken>()), Times.Once);
        _packagerRepoMock.Verify(x => x.UpdateStatusRawAsync(packager.Id, "deactivated", It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(x => x.SendNotificationAsync(packager.UserId, "Packager Application Rejected", It.IsAny<string>(), packager.Id, NotificationType.approval, It.IsAny<CancellationToken>()), Times.Once);

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public async Task GetPendingPackagersAsync_ReturnsMappedList()
    {
        var list = new List<Packager> { new Packager() };
        var responses = new List<PackagerResponse> { new PackagerResponse(Guid.NewGuid(), Guid.NewGuid(), "Company", "123", "Desc", "email", "phone", "url", "pending", null, 0, 0, DateTime.UtcNow, null) };

        _packagerRepoMock.Setup(x => x.GetPendingApprovalAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        _mapperMock.Setup(x => x.Map<IEnumerable<PackagerResponse>>(list)).Returns(responses);

        var result = await _packagerService.GetPendingPackagersAsync();
        result.Should().BeEquivalentTo(responses);
    }

    [Test]
    public async Task GetMyPackagerStatusAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Packager?)null);
        Func<Task> act = async () => await _packagerService.GetMyPackagerStatusAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No packager application found for this user.");
    }

    [Test]
    public async Task GetMyPackagerStatusAsync_Found_ReturnsMappedResponse()
    {
        var packager = new Packager();
        var response = new PackagerResponse(Guid.NewGuid(), Guid.NewGuid(), "Company", "123", "Desc", "email", "phone", "url", "pending", null, 0, 0, DateTime.UtcNow, null);

        _packagerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(packager);
        _mapperMock.Setup(x => x.Map<PackagerResponse>(packager)).Returns(response);

        var result = await _packagerService.GetMyPackagerStatusAsync(Guid.NewGuid());
        result.Should().BeEquivalentTo(response);
    }

    [Test]
    public async Task GetPublicPackagersAsync_ReturnsPagedResponse()
    {
        var request = new PackagerSearchRequest { SearchTerm = "test", PageNumber = 2, PageSize = 10 };
        var list = new List<Packager> { new Packager() };
        var responses = new List<PublicPackagerResponse> { new PublicPackagerResponse { Id = Guid.NewGuid(), CompanyName = "Company", Description = "Desc", WebsiteUrl = "url", AvgRating = 5, TotalReviews = 10, TotalPackagesContributed = 2 } };

        _packagerRepoMock.Setup(x => x.SearchPublicPackagersAsync(request.SearchTerm, request.PageNumber, request.PageSize, It.IsAny<CancellationToken>())).ReturnsAsync((list, 25));
        _mapperMock.Setup(x => x.Map<List<PublicPackagerResponse>>(list)).Returns(responses);

        var result = await _packagerService.GetPublicPackagersAsync(request);

        result.Items.Should().BeEquivalentTo(responses);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
    }
}
