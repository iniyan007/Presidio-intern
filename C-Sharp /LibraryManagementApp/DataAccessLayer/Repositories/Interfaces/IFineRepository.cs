using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces;
public interface IFineRepository
{
    List<FinePayment> GetByMemberId(int memberId);
    decimal? GetTotalUnpaidFine(int memberId);
    void AddPayment(FinePayment payment);
    decimal GetTotalPaidFine(int memberId);

}