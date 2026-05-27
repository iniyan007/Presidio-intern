using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class PackageTransport
{
    public Guid Id { get; set; }

    public Guid ItineraryDayId { get; set; }

    public int SegmentOrder { get; set; }

    public string? VehicleDescription { get; set; }

    public string PickupPoint { get; set; } = null!;

    public string DropPoint { get; set; } = null!;

    public TimeOnly? PickupTime { get; set; }

    public TimeOnly? DropTime { get; set; }

    public decimal? DistanceKm { get; set; }

    public string? Notes { get; set; }

    public virtual ItineraryDay ItineraryDay { get; set; } = null!;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? TransientTransportMode { get; set; }
}
