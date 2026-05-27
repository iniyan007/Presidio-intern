using System;

namespace TravelTourManagement.DataAccess.DTOs.Payments;

public record PaymentResponse(
    Guid Id,
    Guid BookingId,
    decimal Amount,
    string PaymentMethod,
    string TransactionId,
    DateTime PaidAt
);
