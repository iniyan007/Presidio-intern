using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class PackageInclusion
{
    public Guid Id { get; set; }

    public Guid PackageId { get; set; }

    public string Description { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public virtual Package Package { get; set; } = null!;

    public TravelTourManagement.DataAccess.Enums.InclusionType Type { get; set; }
}
