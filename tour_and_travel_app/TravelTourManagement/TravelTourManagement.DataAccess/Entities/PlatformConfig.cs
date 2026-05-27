using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class PlatformConfig
{
    public Guid Id { get; set; }

    public decimal PlatformFeePercent { get; set; }

    public decimal GstPercent { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Note { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
