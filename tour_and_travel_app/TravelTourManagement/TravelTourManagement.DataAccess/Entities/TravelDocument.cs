using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class TravelDocument
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid? TravelerId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string? OriginalFilename { get; set; }

    public long? FileSizeBytes { get; set; }

    public string? MimeType { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public Guid? VerifiedBy { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual BookingTraveler? Traveler { get; set; }

    public virtual User? VerifiedByNavigation { get; set; }
}
