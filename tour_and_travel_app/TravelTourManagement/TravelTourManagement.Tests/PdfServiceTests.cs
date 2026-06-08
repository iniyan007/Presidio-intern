using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using TravelTourManagement.Business.Services;
using TravelTourManagement.DataAccess.Entities;
using TravelTourManagement.DataAccess.Enums;

namespace TravelTourManagement.Tests;

[TestFixture]
public class PdfServiceTests
{
    private PdfService _pdfService;

    [SetUp]
    public void Setup()
    {
        _pdfService = new PdfService();
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Test]
    public void GenerateBookingTicketPdf_ValidBooking_ReturnsPdfBytes()
    {
        // Arrange
        var packager = new Packager
        {
            CompanyName = "Test Company",
            ContactEmail = "test@company.com",
            ContactPhone = "1234567890"
        };

        var package = new Package
        {
            Title = "Test Package",
            City = "Paris",
            Country = "France",
            Packager = packager
        };

        var bookingTravelers = new List<BookingTraveler>
        {
            new BookingTraveler { FullName = "John Doe", Gender = "Male", DateOfBirth = new DateOnly(1990, 1, 1) },
            new BookingTraveler { FullName = "Jane Doe", Gender = "Female", DateOfBirth = new DateOnly(1992, 2, 2) }
        };

        var booking = new Booking
        {
            BookingReference = "REF123456",
            BookedAt = DateTime.UtcNow,
            TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
            TotalAmount = 1500.50m,
            Package = package,
            BookingTravelers = bookingTravelers
        };

        // Act
        var pdfBytes = _pdfService.GenerateBookingTicketPdf(booking);

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(0);
        
        // Basic check for PDF magic number (%PDF)
        pdfBytes[0].Should().Be(0x25); // %
        pdfBytes[1].Should().Be(0x50); // P
        pdfBytes[2].Should().Be(0x44); // D
        pdfBytes[3].Should().Be(0x46); // F
    }

    [Test]
    public void GenerateBookingTicketPdf_MissingOptionalFields_ReturnsPdfBytesWithoutCrashing()
    {
        // Arrange
        var packager = new Packager
        {
            CompanyName = "Test Company"
            // ContactEmail and ContactPhone are null
        };

        var package = new Package
        {
            Title = "Test Package",
            City = "Paris",
            Country = "France",
            Packager = packager
        };

        var bookingTravelers = new List<BookingTraveler>
        {
            new BookingTraveler() // Missing FullName, Gender, DateOfBirth
        };

        var booking = new Booking
        {
            BookingReference = "REF123456",
            BookedAt = DateTime.UtcNow,
            TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
            TotalAmount = 1500.50m,
            Package = package,
            BookingTravelers = bookingTravelers
        };

        // Act
        var pdfBytes = _pdfService.GenerateBookingTicketPdf(booking);

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(0);
    }
}
