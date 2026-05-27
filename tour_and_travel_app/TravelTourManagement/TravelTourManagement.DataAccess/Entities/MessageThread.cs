using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class MessageThread
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid PackagerId { get; set; }

    public Guid? PackageId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Package? Package { get; set; }

    public virtual Packager Packager { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
