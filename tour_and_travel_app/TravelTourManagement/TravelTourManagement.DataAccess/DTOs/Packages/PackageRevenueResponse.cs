using System;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public class PackageRevenueResponse
{
    public Guid PackageId { get; set; }
    public string PackageTitle { get; set; } = null!;
    public decimal Revenue { get; set; }
    public string RevenueType { get; set; } = null!;
    public int TotalConfirmedBookings { get; set; }
}
