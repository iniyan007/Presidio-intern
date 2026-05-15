namespace BusinessLayer.DTOs;

public class FinePaymentDto
{
    public int Id { get; set; }
    public int BorrowId { get; set; }
    public string BookTitle { get; set; } = null!;
    public decimal AmountPaid { get; set; }
    public DateTime PaymentDate { get; set; }
}