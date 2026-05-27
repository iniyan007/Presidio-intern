using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class PackageHighlight
{
    public Guid Id { get; set; }

    public Guid PackageId { get; set; }

    public string HighlightText { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public virtual Package Package { get; set; } = null!;
}
