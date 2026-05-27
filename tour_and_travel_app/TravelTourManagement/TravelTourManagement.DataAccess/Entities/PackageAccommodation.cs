using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class PackageAccommodation
{
    public Guid Id { get; set; }

    public Guid ItineraryDayId { get; set; }

    public string HotelName { get; set; } = null!;

    public string? HotelAddress { get; set; }

    public short? StarRating { get; set; }

    public string? RoomType { get; set; }

    public TimeOnly? CheckInTime { get; set; }

    public TimeOnly? CheckOutTime { get; set; }

    public string? Amenities { get; set; }

    public string? Notes { get; set; }

    public virtual ItineraryDay ItineraryDay { get; set; } = null!;
}
