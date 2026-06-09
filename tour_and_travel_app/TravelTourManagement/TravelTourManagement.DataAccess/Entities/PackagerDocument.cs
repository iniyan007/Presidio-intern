using System;

namespace TravelTourManagement.DataAccess.Entities;

public partial class PackagerDocument
{
    public Guid Id { get; set; }

    public Guid PackagerId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string? OriginalFilename { get; set; }

    public long? FileSizeBytes { get; set; }

    public string? MimeType { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Packager Packager { get; set; } = null!;
}
