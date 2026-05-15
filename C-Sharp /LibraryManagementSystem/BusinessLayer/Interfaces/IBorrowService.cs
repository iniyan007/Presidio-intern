using BusinessLayer.DTOs;

namespace BusinessLayer.Interfaces;

public interface IBorrowService
{
    Task<(bool Success, string Message)> BorrowBookAsync(int memberId, int bookId);
    Task<(bool Success, string Message)> ReturnBookAsync(int borrowId);
    Task<List<BorrowDto>> GetAllBorrowsAsync();
    Task<List<BorrowDto>> GetBorrowsByMemberAsync(int memberId);
    Task<List<BorrowDto>> GetActiveBorrowsAsync();
    Task<List<BorrowDto>> GetOverdueBorrowsAsync();
    Task<BorrowingSummaryDto?> GetMemberBorrowingSummaryAsync(int memberId);
}