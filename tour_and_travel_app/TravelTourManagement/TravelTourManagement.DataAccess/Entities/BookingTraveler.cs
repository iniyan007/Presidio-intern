using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class BookingTraveler
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public string FullName { get; set; } = null!;

    public string? PassportNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Nationality { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<TravelDocument> TravelDocuments { get; set; } = new List<TravelDocument>();
}
