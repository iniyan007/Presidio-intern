namespace BankingAPI.Models.DTOs;

public class WithdrawalRequest
{
    public string FromAccountNumber { get; set; } = string.Empty;
    public float Amount { get; set; }
}