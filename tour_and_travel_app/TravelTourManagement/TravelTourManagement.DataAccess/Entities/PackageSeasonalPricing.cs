using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class PackageSeasonalPricing
{
    public Guid Id { get; set; }

    public Guid PackageId { get; set; }

    public string SeasonName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal BasePrice { get; set; }

    public decimal ChildPrice { get; set; }

    public decimal DiscountPercent { get; set; }

    public int AvailableSlots { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Package Package { get; set; } = null!;
}
