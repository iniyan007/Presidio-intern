using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.DTOs.Bookings;

public record BookingResponse(
    Guid Id,
    Guid UserId,
    Guid PackageId,
    string BookingReference,
    int AdultCount,
    int ChildCount,
    decimal TotalAmount,
    decimal PaidAmount,
    string PaymentStatus, // derived: Unpaid, Partial, Paid
    DateOnly TravelDate,
    DateOnly ReturnDate,
    string? SpecialRequests,
    DateTime BookedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    List<BookingTravelerResponse> Travelers
);

public record BookingTravelerResponse(
    Guid Id,
    string FullName,
    string? PassportNumber,
    DateOnly? DateOfBirth,
    string? Nationality,
    bool IsPrimary
);
