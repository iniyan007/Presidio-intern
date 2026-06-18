using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.DTOs.Reviews;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public short OverallRating { get; set; }
    public short? AccommodationRating { get; set; }
    public short? TransportRating { get; set; }
    public short? FoodRating { get; set; }
    public short? GuideRating { get; set; }
    public short? ValueRating { get; set; }
    public string? Comment { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public bool IsVerifiedTraveler { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReviewMediaResponse> Media { get; set; } = new();
}

public class ReviewMediaResponse
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
}
