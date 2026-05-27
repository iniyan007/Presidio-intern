using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class ItineraryActivity
{
    public Guid Id { get; set; }

    public Guid ItineraryDayId { get; set; }

    public int SequenceOrder { get; set; }

    public string ActivityTitle { get; set; } = null!;

    public string? Description { get; set; }

    public string? ActivityType { get; set; }

    public string? Location { get; set; }

    public int? DurationMinutes { get; set; }

    public bool IsOptional { get; set; }

    public decimal ExtraCost { get; set; }

    public virtual ItineraryDay ItineraryDay { get; set; } = null!;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? TransientDaySession { get; set; }
}
