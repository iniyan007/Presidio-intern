using System;
using System.Collections.Generic;

namespace TravelTourManagement.DataAccess.Entities;

public partial class PackageMedium
{
    public Guid Id { get; set; }

    public Guid PackageId { get; set; }

    public string FilePath { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string? Caption { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPrimary { get; set; }

    public long? FileSizeBytes { get; set; }

    public string? MimeType { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Package Package { get; set; } = null!;

    public TravelTourManagement.DataAccess.Enums.MediaCategory Category { get; set; }
}
