using BusinessLayer.DTOs;

namespace BusinessLayer.Interfaces;

public interface IFineService
{
    Task<decimal> GetUnpaidFineAsync(int memberId);
    Task<List<FinePaymentDto>> GetFineHistoryAsync(int memberId);
    Task<(bool Success, string Message)> PayFineAsync(int memberId, decimal amount);
    Task<FineDto> GetFineSummaryAsync(int memberId);
}