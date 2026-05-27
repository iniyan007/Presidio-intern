using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class Review
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid UserId { get; set; }

    public Guid PackageId { get; set; }

    public Guid PackagerId { get; set; }

    public short OverallRating { get; set; }

    public short? AccommodationRating { get; set; }

    public short? TransportRating { get; set; }

    public short? FoodRating { get; set; }

    public short? GuideRating { get; set; }

    public short? ValueRating { get; set; }

    public string? Comment { get; set; }

    public bool IsVerifiedTraveler { get; set; }

    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Package Package { get; set; } = null!;

    public virtual Packager Packager { get; set; } = null!;

    public virtual ICollection<ReviewMedium> ReviewMedia { get; set; } = new List<ReviewMedium>();

    public virtual User User { get; set; } = null!;
}
