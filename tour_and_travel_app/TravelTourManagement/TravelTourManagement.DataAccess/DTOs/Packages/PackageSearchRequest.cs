using System;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Packages;

public class PackageSearchRequest
{
    public string? SearchTerm { get; set; }
    public string? Destination { get; set; }
    public string? Country { get; set; }
    public string? PackageType { get; set; }
    public string? PackagerName { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; set; }

    [Range(1, 365)]
    public int? MinDurationDays { get; set; }

    [Range(1, 365)]
    public int? MaxDurationDays { get; set; }

    public DateOnly? TravelStartDate { get; set; }
    public DateOnly? TravelEndDate { get; set; }

    // Sort options: PriceAsc, PriceDesc, RatingDesc, DurationAsc, DurationDesc, Newest
    public string? SortBy { get; set; } = "Newest";

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
