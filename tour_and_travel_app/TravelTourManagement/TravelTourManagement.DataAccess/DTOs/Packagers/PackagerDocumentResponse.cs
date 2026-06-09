using System;

namespace TravelTourManagement.DataAccess.DTOs.Packagers;

public class PackagerDocumentResponse
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string? OriginalFilename { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public DateTime UploadedAt { get; set; }
    public string FileUrl { get; set; } = null!;
}
