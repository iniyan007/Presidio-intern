using System;

namespace TravelTourManagement.DataAccess.DTOs.Packagers;

public class PublicPackagerResponse
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = null!;
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? ContactEmail { get; set; }
    public decimal AvgRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalPackagesContributed { get; set; }
}
