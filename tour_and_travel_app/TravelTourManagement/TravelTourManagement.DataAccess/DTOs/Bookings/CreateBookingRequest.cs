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

    [Range(0, 100, ErrorMessage = "Infant count must be between 0 and 100.")]
    public int InfantCount { get; set; } = 0;

    [MaxLength(500)]
    public string? SpecialRequests { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one traveler is required.")]
    public List<BookingTravelerRequest> Travelers { get; set; } = new();
}

public class BookingTravelerRequest : IValidatableObject
{
    [Required(ErrorMessage = "Traveler Full Name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Date of Birth is required.")]
    public DateOnly? DateOfBirth { get; set; }

    [Range(0, 120, ErrorMessage = "Age must be between 0 and 120.")]
    public int? Age { get; set; }

    public string? Gender { get; set; }

    // Optional for Indians travelling within India, required otherwise.
    // If provided, could be validated.
    public string? PassportNumber { get; set; }

    // Required for all Indians
    [Required(ErrorMessage = "Aadhar Card Number is required for all travelers.")]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "Invalid Aadhar Card Number. It must be exactly 12 digits.")]
    public string? AadharCardNumber { get; set; }

    public string? Nationality { get; set; }

    public string? MealPreference { get; set; }

    public bool IsPrimary { get; set; }

    public string? AadharCardFileName { get; set; }

    public string? PassportFileName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        if (DateOfBirth.HasValue && DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            results.Add(new ValidationResult("Date of Birth cannot be in the future.", new[] { nameof(DateOfBirth) }));
        }

        // We can't easily validate Passport requirement based on Package Destination here because we don't have DB access in DTO validation.
        // We will enforce the Aadhar card requirement statically above, which covers the "Indian users" constraint.

        return results;
    }
}
