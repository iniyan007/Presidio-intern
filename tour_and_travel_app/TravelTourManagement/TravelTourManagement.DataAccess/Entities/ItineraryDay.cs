using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class ItineraryDay
{
    public Guid Id { get; set; }

    public Guid PackageId { get; set; }

    public int DayNumber { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ItineraryActivity> ItineraryActivities { get; set; } = new List<ItineraryActivity>();

    public virtual ICollection<ItineraryDayMeal> ItineraryDayMeals { get; set; } = new List<ItineraryDayMeal>();

    public virtual Package Package { get; set; } = null!;

    public virtual ICollection<PackageAccommodation> PackageAccommodations { get; set; } = new List<PackageAccommodation>();

    public virtual ICollection<PackageTransport> PackageTransports { get; set; } = new List<PackageTransport>();
}
