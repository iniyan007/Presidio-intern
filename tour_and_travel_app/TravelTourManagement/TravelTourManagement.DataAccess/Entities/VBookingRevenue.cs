using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class VBookingRevenue
{
    public Guid? BookingId { get; set; }

    public string? BookingReference { get; set; }

    public DateTime? BookedAt { get; set; }

    public DateOnly? TravelDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public Guid? PackageId { get; set; }

    public string? PackageTitle { get; set; }

    public string? Destination { get; set; }

    public Guid? PackagerId { get; set; }

    public string? PackagerName { get; set; }

    public Guid? UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserEmail { get; set; }

    public string? UserPhone { get; set; }

    public int? AdultCount { get; set; }

    public int? ChildCount { get; set; }

    public decimal? PackagerBaseAmount { get; set; }

    public decimal? PlatformFeePercent { get; set; }

    public decimal? PlatformFeeAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? PaidAmount { get; set; }
}
