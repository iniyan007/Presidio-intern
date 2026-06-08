using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TravelTourManagement.DataAccess.Context;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;
using TravelTourManagement.DataAccess.Repository;

namespace TravelTourManagement.Tests;

[TestFixture]
public class BookingRepositoryTests
{
    private ApplicationDbContext _dbContext;
    private BookingRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new BookingRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private Booking CreateTestBooking(Guid userId, string reference, BookingStatus status, DateTime bookedAt)
    {
        var packageId = Guid.NewGuid();
        var package = new Package
        {
            Id = packageId,
            Title = "Test Package",
            Destination = "Dest",
            Country = "Country",
            Status = PackageStatus.Published,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Packager = new Packager
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CompanyName = "Test Packager"
            }
        };

        _dbContext.Packages.Add(package);

        return new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PackageId = packageId,
            SeasonalPricingId = Guid.NewGuid(),
            BookingReference = reference,
            AdultCount = 2,
            ChildCount = 0,
            InfantCount = 0,
            TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35)),
            PackagerBaseAmount = 1000,
            PlatformFeePercent = 5,
            PlatformFeeAmount = 50,
            TaxAmount = 100,
            TotalAmount = 1150,
            PaidAmount = 0,
            Status = status,
            PaymentStatus = PaymentStatus.Unpaid,
            BookedAt = bookedAt,
            UpdatedAt = bookedAt,
            BookingTravelers = new List<BookingTraveler>
            {
                new BookingTraveler
                {
                    Id = Guid.NewGuid(),
                    FullName = "John Doe",
                    IsPrimary = true
                }
            }
        };
    }

    [Test]
    public async Task GetByUserIdAsync_ReturnsUserBookingsOrderedByDateDesc()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldBooking = CreateTestBooking(userId, "BKG-OLD", BookingStatus.Completed, DateTime.UtcNow.AddDays(-10));
        var newBooking = CreateTestBooking(userId, "BKG-NEW", BookingStatus.Pending, DateTime.UtcNow.AddDays(-1));
        var otherUserBooking = CreateTestBooking(Guid.NewGuid(), "BKG-OTHER", BookingStatus.Pending, DateTime.UtcNow);

        await _dbContext.Bookings.AddRangeAsync(oldBooking, newBooking, otherUserBooking);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result[0].BookingReference.Should().Be("BKG-NEW"); // Newest first
        result[1].BookingReference.Should().Be("BKG-OLD");
        result.All(b => b.Package != null).Should().BeTrue(); // Ensures Package is included
    }

    [Test]
    public async Task GetWithFullDetailsAsync_ExistingId_IncludesTravelersAndPackage()
    {
        // Arrange
        var booking = CreateTestBooking(Guid.NewGuid(), "BKG-FULL", BookingStatus.Pending, DateTime.UtcNow);
        await _dbContext.Bookings.AddAsync(booking);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithFullDetailsAsync(booking.Id);

        // Assert
        result.Should().NotBeNull();
        result!.BookingTravelers.Should().HaveCount(1);
        result.Package.Should().NotBeNull();
        result.Package!.Title.Should().Be("Test Package");
    }

    [Test]
    public async Task GetExpiredPendingBookingsAsync_ReturnsOnlyPendingBookingsPastCutoff()
    {
        // Arrange
        var cutoffTime = DateTime.UtcNow.AddMinutes(-5);
        
        // This one is pending and older than 5 minutes -> Should be returned
        var expiredPending = CreateTestBooking(Guid.NewGuid(), "BKG-EXP-PEN", BookingStatus.Pending, DateTime.UtcNow.AddMinutes(-10));
        
        // This one is pending but recent -> Should NOT be returned
        var recentPending = CreateTestBooking(Guid.NewGuid(), "BKG-REC-PEN", BookingStatus.Pending, DateTime.UtcNow.AddMinutes(-1));
        
        // This one is older than 5 minutes but already confirmed -> Should NOT be returned
        var expiredConfirmed = CreateTestBooking(Guid.NewGuid(), "BKG-EXP-CON", BookingStatus.Confirmed, DateTime.UtcNow.AddMinutes(-10));

        await _dbContext.Bookings.AddRangeAsync(expiredPending, recentPending, expiredConfirmed);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetExpiredPendingBookingsAsync(cutoffTime);

        // Assert
        result.Should().HaveCount(1);
        result.First().BookingReference.Should().Be("BKG-EXP-PEN");
    }

    [Test]
    public async Task ReferenceExistsAsync_ExistingReference_ReturnsTrue()
    {
        // Arrange
        var booking = CreateTestBooking(Guid.NewGuid(), "BKG-UNIQUE", BookingStatus.Pending, DateTime.UtcNow);
        await _dbContext.Bookings.AddAsync(booking);
        await _dbContext.SaveChangesAsync();

        // Act
        var exists = await _repository.ReferenceExistsAsync("BKG-UNIQUE");
        var notExists = await _repository.ReferenceExistsAsync("BKG-NON-EXISTENT");

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }
}
