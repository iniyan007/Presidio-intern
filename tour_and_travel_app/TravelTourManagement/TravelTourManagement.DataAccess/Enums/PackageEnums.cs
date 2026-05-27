using System;

namespace TravelTourManagement.DataAccess.Enums;

public enum PackageType
{
    Group,
    Private,
    Honeymoon,
    Family,
    Adventure,
    Pilgrimage
}

public enum PackageStatus
{
    Draft,
    PendingReview,
    Published,
    Archived
}

public enum InclusionType
{
    Included,
    Excluded,
    Optional
}

public enum MediaCategory
{
    Hotel,
    Transport,
    Food,
    Destination,
    Activity,
    Cover
}

public enum MealType
{
    Breakfast,
    Lunch,
    Dinner,
    AllInclusive,
    None
}

public enum TransportMode
{
    Bus,
    Train,
    Flight,
    Car,
    Boat,
    Van,
    Walk,
    Other
}

public enum DaySession
{
    Morning,
    Afternoon,
    Evening,
    FullDay
}
