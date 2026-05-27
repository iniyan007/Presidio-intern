using BusinessLayer.DTOs;

namespace BusinessLayer.Interfaces;
public interface IBorrowService
{
    (bool Success, string Message) BorrowBook(int memberId, int bookId);
    (bool Success, string Message) ReturnBook(int borrowId);
    List<BorrowDto> GetAllBorrows();
    List<BorrowDto> GetBorrowsByMember(int memberId);
    List<BorrowDto> GetActiveBorrows();
    List<BorrowDto> GetOverdueBorrows();
    BorrowingSummaryDto? GetMemberBorrowingSummary(int memberId);
}