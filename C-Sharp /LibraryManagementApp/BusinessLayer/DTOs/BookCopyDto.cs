using DataAccessLayer.Enums;

namespace BusinessLayer.DTOs;

public class BookCopyDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = null!;
    public CopyStatus Status { get; set; }
    public string? Remarks { get; set; }
}