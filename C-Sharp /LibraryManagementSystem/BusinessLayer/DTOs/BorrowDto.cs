using DataAccessLayer.Enums;

namespace BusinessLayer.DTOs;

public class BorrowDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = null!;
    public int BookCopyId { get; set; }
    public string BookTitle { get; set; } = null!;
    public string BookAuthor { get; set; } = null!;
    public string CategoryName { get; set; } = null!;       
    public string? CopyRemarks { get; set; }            
    public DateOnly DateOfBorrow { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? DateOfReturn { get; set; }
    public decimal FineAmount { get; set; }
    public BorrowStatus Status { get; set; }
    public bool IsOverdue => Status == BorrowStatus.Borrowed && DueDate < DateOnly.FromDateTime(DateTime.Today);
    public int OverdueDays => IsOverdue ? DateOnly.FromDateTime(DateTime.Today).DayNumber - DueDate.DayNumber : 0;
}