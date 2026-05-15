using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IFineRepository
{
    Task<List<FinePayment>> GetByMemberIdAsync(int memberId);
    Task<decimal> GetTotalUnpaidFineAsync(int memberId);
    Task AddPaymentAsync(FinePayment payment);
    Task<decimal> GetTotalPaidFineAsync(int memberId);
}