using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces;
public interface IBorrowRepository
{
    List<Borrow> GetAll();
    List<Borrow> GetBorrowsByMemberId(int memberId);
    List<Borrow> GetActiveBorrowsByMemberId(int memberId);
    List<Borrow> GetActiveBorrowByMemberAndBook(int memberId, int bookId);
    List<Borrow> GetOverdueBorrows();
    void AddBorrow(Borrow borrow);
    void UpdateBorrow(Borrow borrow);
}