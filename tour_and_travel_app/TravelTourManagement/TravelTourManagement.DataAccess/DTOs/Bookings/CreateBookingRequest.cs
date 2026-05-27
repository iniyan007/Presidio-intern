using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.DTOs.Bookings;

public record CreateBookingRequest(
    Guid PackageId,
    Guid SeasonalPricingId,
    int AdultCount,
    int ChildCount,
    DateOnly TravelDate,
    string? SpecialRequests,
    List<BookingTravelerRequest> Travelers
);

public record BookingTravelerRequest(
    string FullName,
    string? PassportNumber,
    DateOnly? DateOfBirth,
    string? Nationality,
    bool IsPrimary
);
