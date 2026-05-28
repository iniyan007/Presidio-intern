using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Bookings;

public class CreateBookingRequest
{
    [Required]
    public Guid PackageId { get; set; }

    [Required]
    public Guid SeasonalPricingId { get; set; }

    [Required]
    public DateOnly TravelDate { get; set; }

    [Range(0, 100)]
    public int InfantCount { get; set; } = 0;

    public string? SpecialRequests { get; set; }

    [Required]
    public List<BookingTravelerRequest> Travelers { get; set; } = new();
}

public class BookingTravelerRequest
{
    [Required]
    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public string? PassportNumber { get; set; }

    public string? AadharCardNumber { get; set; }

    public string? Nationality { get; set; }

    public string? MealPreference { get; set; }

    public bool IsPrimary { get; set; }

    public IFormFile? AadharCardFile { get; set; }

    public IFormFile? PassportFile { get; set; }
}


