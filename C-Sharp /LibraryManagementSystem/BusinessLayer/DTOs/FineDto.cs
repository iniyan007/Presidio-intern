namespace BusinessLayer.DTOs;

public class FineDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = null!;
    public decimal TotalFine { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal UnpaidFine { get; set; }
}