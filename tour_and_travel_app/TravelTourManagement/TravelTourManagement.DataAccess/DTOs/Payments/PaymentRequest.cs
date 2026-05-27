using System;

namespace TravelTourManagement.DataAccess.DTOs.Payments;

public record PaymentRequest(
    Guid BookingId,
    decimal Amount,
    string PaymentMethod,
    string TransactionId // This could be a token from Stripe/PayPal depending on implementation
);
