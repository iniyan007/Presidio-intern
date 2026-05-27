using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class Packager
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? BusinessLicenseNo { get; set; }

    public string? Description { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? WebsiteUrl { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? DeactivatedAt { get; set; }

    public string? DeactivationReason { get; set; }

    public decimal AvgRating { get; set; }

    public int TotalReviews { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual ICollection<MessageThread> MessageThreads { get; set; } = new List<MessageThread>();

    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User User { get; set; } = null!;
}
