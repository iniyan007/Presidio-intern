using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.DTOs.Packages;
using TravelTourManagement.DataAccess.DTOs.Wishlists;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.Tests;

[TestFixture]
public class WishlistServiceTests
{
    private ApplicationDbContext _dbContext;
    private Mock<IMapper> _mapperMock;
    private WishlistService _wishlistService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _mapperMock = new Mock<IMapper>();

        _wishlistService = new WishlistService(_dbContext, _mapperMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    // --- ToggleWishlistAsync ---

    [Test]
    public async Task ToggleWishlistAsync_PackageNotFound_ThrowsKeyNotFoundException()
    {
        Func<Task> act = async () => await _wishlistService.ToggleWishlistAsync(Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<System.Collections.Generic.KeyNotFoundException>().WithMessage("Package not found or not published.");
    }

    [Test]
    public async Task ToggleWishlistAsync_PackageNotPublished_ThrowsKeyNotFoundException()
    {
        var package = new Package { Id = Guid.NewGuid(), Title = "Test", Destination = "Dest", Country = "Country", Status = PackageStatus.Draft };
        await _dbContext.Packages.AddAsync(package);
        await _dbContext.SaveChangesAsync();

        Func<Task> act = async () => await _wishlistService.ToggleWishlistAsync(Guid.NewGuid(), package.Id);
        await act.Should().ThrowAsync<System.Collections.Generic.KeyNotFoundException>().WithMessage("Package not found or not published.");
    }

    [Test]
    public async Task ToggleWishlistAsync_ValidRequest_AddsToWishlistIfMissing_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var package = new Package { Id = Guid.NewGuid(), Title = "Test", Destination = "Dest", Country = "Country", Status = PackageStatus.Published };
        await _dbContext.Packages.AddAsync(package);
        await _dbContext.SaveChangesAsync();

        var result = await _wishlistService.ToggleWishlistAsync(userId, package.Id);

        result.Should().BeTrue();
        
        var wishlist = await _dbContext.Wishlists.FirstOrDefaultAsync();
        wishlist.Should().NotBeNull();
        wishlist!.UserId.Should().Be(userId);
        wishlist.PackageId.Should().Be(package.Id);
    }

    [Test]
    public async Task ToggleWishlistAsync_ValidRequest_RemovesFromWishlistIfExists_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var package = new Package { Id = Guid.NewGuid(), Title = "Test", Destination = "Dest", Country = "Country", Status = PackageStatus.Published };
        var wishlist = new Wishlist { Id = Guid.NewGuid(), UserId = userId, PackageId = package.Id };
        
        await _dbContext.Packages.AddAsync(package);
        await _dbContext.Wishlists.AddAsync(wishlist);
        await _dbContext.SaveChangesAsync();

        var result = await _wishlistService.ToggleWishlistAsync(userId, package.Id);

        result.Should().BeFalse(); // Indicated removed
        
        var count = await _dbContext.Wishlists.CountAsync();
        count.Should().Be(0); // Successfully removed
    }

    // --- GetUserWishlistsAsync ---

    [Test]
    public async Task GetUserWishlistsAsync_ReturnsOrderedMappedList()
    {
        var userId = Guid.NewGuid();
        var packager = new Packager { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CompanyName = "Comp" };
        await _dbContext.Packagers.AddAsync(packager);

        var package1 = new Package { Id = Guid.NewGuid(), Title = "Pkg 1", Destination = "Dest 1", Country = "Country", Status = PackageStatus.Published, PackagerId = packager.Id, Packager = packager };
        var package2 = new Package { Id = Guid.NewGuid(), Title = "Pkg 2", Destination = "Dest 2", Country = "Country", Status = PackageStatus.Draft, PackagerId = packager.Id, Packager = packager }; // Should be ignored
        var package3 = new Package { Id = Guid.NewGuid(), Title = "Pkg 3", Destination = "Dest 3", Country = "Country", Status = PackageStatus.Published, PackagerId = packager.Id, Packager = packager };
        
        await _dbContext.Packages.AddRangeAsync(package1, package2, package3);

        var wishlist1 = new Wishlist { Id = Guid.NewGuid(), UserId = userId, PackageId = package1.Id, Package = package1, CreatedAt = DateTime.UtcNow.AddMinutes(1) };
        var wishlist2 = new Wishlist { Id = Guid.NewGuid(), UserId = userId, PackageId = package2.Id, Package = package2, CreatedAt = DateTime.UtcNow.AddMinutes(2) };
        var wishlist3 = new Wishlist { Id = Guid.NewGuid(), UserId = userId, PackageId = package3.Id, Package = package3, CreatedAt = DateTime.UtcNow.AddMinutes(3) }; // Newest
        var wishlistOtherUser = new Wishlist { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PackageId = package1.Id, Package = package1, CreatedAt = DateTime.UtcNow.AddMinutes(4) };

        await _dbContext.Wishlists.AddRangeAsync(wishlist1, wishlist2, wishlist3, wishlistOtherUser);
        await _dbContext.SaveChangesAsync();

        var packageSummary1 = new PackageSummaryResponse(package1.Id, Guid.NewGuid(), "Company", "Pkg 1", "Type", "Dest 1", "Country", 1, 1, 5, 10, null, 100, 10, null, null);
        var packageSummary3 = new PackageSummaryResponse(package3.Id, Guid.NewGuid(), "Company", "Pkg 3", "Type", "Dest 3", "Country", 1, 1, 5, 10, null, 100, 10, null, null);

        _mapperMock.Setup(x => x.Map<PackageSummaryResponse>(It.Is<Package>(p => p.Id == package1.Id))).Returns(packageSummary1);
        _mapperMock.Setup(x => x.Map<PackageSummaryResponse>(It.Is<Package>(p => p.Id == package3.Id))).Returns(packageSummary3);

        var result = (await _wishlistService.GetUserWishlistsAsync(userId)).ToList();

        result.Should().HaveCount(2); // Only 1 and 3 (2 is draft, 4 is other user)
        result[0].PackageId.Should().Be(package3.Id); // Descending order of CreatedAt
        result[1].PackageId.Should().Be(package1.Id);
        
        result[0].Package.Title.Should().Be("Pkg 3");
        result[1].Package.Title.Should().Be("Pkg 1");
    }
}
