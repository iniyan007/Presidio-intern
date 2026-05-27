using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class ItineraryDayMeal
{
    public Guid Id { get; set; }

    public Guid ItineraryDayId { get; set; }

    public string? Venue { get; set; }

    public string? Description { get; set; }

    public bool IsIncluded { get; set; }

    public virtual ItineraryDay ItineraryDay { get; set; } = null!;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? TransientMealType { get; set; }
}
