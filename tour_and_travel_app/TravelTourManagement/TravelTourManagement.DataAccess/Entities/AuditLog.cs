using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class AuditLog
{
    public Guid Id { get; set; }

    public Guid? PerformedBy { get; set; }

    public string EntityType { get; set; } = null!;

    public Guid EntityId { get; set; }

    public string Action { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime PerformedAt { get; set; }

    public virtual User? PerformedByNavigation { get; set; }
}
