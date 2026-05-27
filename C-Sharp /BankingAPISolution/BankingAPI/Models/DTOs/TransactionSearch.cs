namespace BankingAPI.Models.DTOs;
public class TransactionSearch
{
    public string? Search { get; set; } = null;

    public string? Status { get; set; } = null;

    public decimal? MinAmount { get; set; } = null;

    public decimal? MaxAmount { get; set; } = decimal.MaxValue;

    public DateTime? StartDate { get; set; } = DateTime.Now;

    public DateTime? EndDate { get; set; } = DateTime.Now;

    public string SortBy { get; set; } = "TransactionDate";

    public string SortOrder { get; set; } = "desc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}