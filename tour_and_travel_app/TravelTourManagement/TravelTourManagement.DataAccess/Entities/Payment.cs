using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public string TransactionId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public string? GatewayResponse { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? RefundedAt { get; set; }

    public decimal? RefundAmount { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
