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
    int InfantCount,
    decimal TotalAmount,
    decimal PaidAmount,
    string Status, // e.g., Pending, DocumentUnderReview, Confirmed, Cancelled
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
    int? Age,
    string? Gender,
    string? MealPreference,
    string? AadharCardNumber,
    bool IsPrimary,
    List<TravelDocumentResponse> Documents
);

public record TravelDocumentResponse(
    Guid Id,
    string DocumentType,
    string FilePath,
    string FileName,
    DateTime UploadedAt,
    string Status,
    string? RejectionReason
);
