namespace BankingAPI.Models.DTOs
{
    public class TransactionRequest
    {
        public string FromAccountNumber { get; set; } = string.Empty;
        public string ToAccountNumber { get; set; } = string.Empty;
        public float Amount { get; set; }
    }
}