using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class ReviewMedium
{
    public Guid Id { get; set; }

    public Guid ReviewId { get; set; }

    public string FilePath { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public virtual Review Review { get; set; } = null!;
}
