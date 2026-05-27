using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class VPackagerRevenueSummary
{
    public Guid? PackagerId { get; set; }

    public string? CompanyName { get; set; }

    public long? TotalBookings { get; set; }

    public long? ConfirmedBookings { get; set; }

    public long? CompletedBookings { get; set; }

    public long? CancelledBookings { get; set; }

    public decimal? TotalEarned { get; set; }

    public decimal? TotalPlatformFee { get; set; }

    public decimal? TotalGmv { get; set; }
}
