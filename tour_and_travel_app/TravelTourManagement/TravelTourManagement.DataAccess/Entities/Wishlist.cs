using System;

namespace TravelTourManagement.DataAccess.Entities;

public class Wishlist
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PackageId { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Package Package { get; set; } = null!;
}
