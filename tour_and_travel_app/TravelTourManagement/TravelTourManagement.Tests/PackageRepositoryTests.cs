using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.DTOs.Packages;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Repository;

namespace TravelTourManagement.Tests;

[TestFixture]
public class PackageRepositoryTests
{
    private ApplicationDbContext _dbContext;
    private PackageRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new PackageRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private Package CreateTestPackage(string title, string destination, decimal basePrice, PackageStatus status = PackageStatus.Published)
    {
        return new Package
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Test desc",
            Destination = destination,
            Country = "TestCountry",
            DurationDays = 5,
            DurationNights = 4,
            Type = PackageType.Family,
            MaxCapacity = 10,
            CurrentBookings = 0,
            Status = status,
            AvgRating = 4.5m,
            IsFeatured = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PackageSeasonalPricings = new List<PackageSeasonalPricing>
            {
                new PackageSeasonalPricing
                {
                    Id = Guid.NewGuid(),
                    SeasonName = "Test Season",
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                    EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                    BasePrice = basePrice,
                    ChildPrice = basePrice * 0.5m,
                    AvailableSlots = 10,
                    IsActive = true
                }
            },
            ItineraryDays = new List<ItineraryDay>
            {
                new ItineraryDay
                {
                    Id = Guid.NewGuid(),
                    DayNumber = 1,
                    Title = "Day 1",
                    Description = "Desc 1"
                }
            },
            Packager = new Packager
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CompanyName = "Test Packager"
            }
        };
    }

    [Test]
    public async Task SearchPackagesAsync_WithPagination_ReturnsCorrectPageSize()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            await _dbContext.Packages.AddAsync(CreateTestPackage($"Package {i}", "Paris", 1000));
        }
        await _dbContext.SaveChangesAsync();

        var request = new PackageSearchRequest
        {
            PageNumber = 2,
            PageSize = 5
        };

        // Act
        var (packages, totalCount) = await _repository.SearchPackagesAsync(request);

        // Assert
        totalCount.Should().Be(15);
        packages.Should().HaveCount(5);
    }

    [Test]
    public async Task SearchPackagesAsync_FilterByDestination_ReturnsMatchingPackages()
    {
        // Arrange
        await _dbContext.Packages.AddAsync(CreateTestPackage("Rome Trip", "Rome", 1000));
        await _dbContext.Packages.AddAsync(CreateTestPackage("Paris Trip", "Paris", 1200));
        await _dbContext.SaveChangesAsync();

        var request = new PackageSearchRequest
        {
            Destination = "Rome",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (packages, totalCount) = await _repository.SearchPackagesAsync(request);

        // Assert
        totalCount.Should().Be(1);
        packages.First().Destination.Should().Be("Rome");
    }

    [Test]
    public async Task SearchPackagesAsync_SortByPriceAsc_ReturnsSortedPackages()
    {
        // Arrange
        await _dbContext.Packages.AddAsync(CreateTestPackage("Expensive", "Rome", 5000));
        await _dbContext.Packages.AddAsync(CreateTestPackage("Cheap", "Paris", 1000));
        await _dbContext.Packages.AddAsync(CreateTestPackage("Medium", "London", 2500));
        await _dbContext.SaveChangesAsync();

        var request = new PackageSearchRequest
        {
            SortBy = "priceasc",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (packages, totalCount) = await _repository.SearchPackagesAsync(request);

        // Assert
        totalCount.Should().Be(3);
        packages[0].Title.Should().Be("Cheap");
        packages[1].Title.Should().Be("Medium");
        packages[2].Title.Should().Be("Expensive");
    }

    [Test]
    public async Task GetWithFullDetailsAsync_ExistingId_IncludesAllNestedEntities()
    {
        // Arrange
        var package = CreateTestPackage("Full Package", "Dubai", 2000);
        await _dbContext.Packages.AddAsync(package);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithFullDetailsAsync(package.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ItineraryDays.Should().HaveCount(1);
        result.PackageSeasonalPricings.Should().HaveCount(1);
    }

    [Test]
    public async Task GetAvailableByDateRangeAsync_ValidDates_ReturnsPackagesWithActivePricing()
    {
        // Arrange
        var package = CreateTestPackage("Valid Package", "Tokyo", 1500);
        await _dbContext.Packages.AddAsync(package);
        await _dbContext.SaveChangesAsync();

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        // Act
        var result = await _repository.GetAvailableByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Valid Package");
    }
}
