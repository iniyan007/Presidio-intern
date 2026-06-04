using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class Package
{
    public Guid Id { get; set; }

    public Guid PackagerId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Destination { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string? City { get; set; }

    public int DurationDays { get; set; }

    public int DurationNights { get; set; }

    public int MaxCapacity { get; set; }

    public int CurrentBookings { get; set; }

    public int? MinAge { get; set; }

    public string? CancellationPolicy { get; set; }

    public bool IsFeatured { get; set; }

    public decimal AvgRating { get; set; }

    public int TotalReviews { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<ItineraryDay> ItineraryDays { get; set; } = new List<ItineraryDay>();

    public virtual ICollection<MessageThread> MessageThreads { get; set; } = new List<MessageThread>();

    public virtual ICollection<PackageHighlight> PackageHighlights { get; set; } = new List<PackageHighlight>();

    public virtual ICollection<PackageInclusion> PackageInclusions { get; set; } = new List<PackageInclusion>();

    public virtual ICollection<PackageMedium> PackageMedia { get; set; } = new List<PackageMedium>();

    public virtual ICollection<PackageSeasonalPricing> PackageSeasonalPricings { get; set; } = new List<PackageSeasonalPricing>();

    public virtual Packager Packager { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

    public TravelTourManagement.DataAccess.Enums.PackageType Type { get; set; }

    public TravelTourManagement.DataAccess.Enums.PackageStatus Status { get; set; }
}
