using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IBorrowRepository
{
    Task<List<Borrow>> GetAllAsync();
    Task<Borrow?> GetByIdAsync(int id);
    Task<List<Borrow>> GetByMemberIdAsync(int memberId);
    Task<List<Borrow>> GetActiveBorrowsByMemberIdAsync(int memberId);
    Task<Borrow?> GetActiveBorrowByMemberAndBookAsync(int memberId, int bookId);
    Task<List<Borrow>> GetOverdueBorrowsAsync();
    Task AddAsync(Borrow borrow);
    Task UpdateAsync(Borrow borrow);
}