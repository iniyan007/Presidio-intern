using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class Booking
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid PackageId { get; set; }

    public Guid SeasonalPricingId { get; set; }

    public string BookingReference { get; set; } = null!;

    public int AdultCount { get; set; }

    public int ChildCount { get; set; }

    public decimal PackagerBaseAmount { get; set; }

    public decimal PlatformFeePercent { get; set; }

    public decimal PlatformFeeAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public DateOnly TravelDate { get; set; }

    public DateOnly ReturnDate { get; set; }

    public string? SpecialRequests { get; set; }

    public DateTime BookedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public virtual ICollection<BookingTraveler> BookingTravelers { get; set; } = new List<BookingTraveler>();

    public virtual Package Package { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Review? Review { get; set; }

    public virtual PackageSeasonalPricing SeasonalPricing { get; set; } = null!;

    public virtual ICollection<TravelDocument> TravelDocuments { get; set; } = new List<TravelDocument>();

    public virtual User User { get; set; } = null!;
}
