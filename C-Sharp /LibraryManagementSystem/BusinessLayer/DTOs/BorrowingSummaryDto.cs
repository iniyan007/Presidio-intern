namespace BusinessLayer.DTOs;

public class BorrowingSummaryDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = null!;
    public int ActiveBorrowings { get; set; }
    public int ReturnedBorrowings { get; set; }
    public decimal TotalUnpaidFine { get; set; }
    public List<BorrowDto> ActiveBooks { get; set; } = new();
}