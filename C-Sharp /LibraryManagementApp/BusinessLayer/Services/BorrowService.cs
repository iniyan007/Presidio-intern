using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;

namespace BusinessLayer.Services;

public class BorrowService : IBorrowService
{
    private readonly IBorrowRepository _borrowRepo;
    private readonly IMemberRepository _memberRepo;
    private readonly IBookRepository _bookRepo;
    private readonly IFineRepository _fineRepo;
    private readonly AppDbContext _context;

    private const decimal FinePerDay = 10m;
    private const decimal MaxUnpaidFine = 500m;

    public BorrowService(
        IBorrowRepository borrowRepo,
        IMemberRepository memberRepo,
        IBookRepository bookRepo,
        IFineRepository fineRepo,
        AppDbContext context)
    {
        _borrowRepo = borrowRepo;
        _memberRepo = memberRepo;
        _bookRepo = bookRepo;
        _fineRepo = fineRepo;
        _context = context;
    }

    public (bool Success, string Message) BorrowBook(int memberId, int bookId)
    {
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            // Step 1: Validate member
            var member = _memberRepo.GetMemberById(memberId);

            if (member == null)
                return (false, "Member not found.");

            if (member.Status == (int)MemberStatus.Inactive)
                return (false, "Member is inactive.");

            // Step 2: Check unpaid fine
            var unpaidFine = _fineRepo.GetTotalUnpaidFine(memberId);

            if (unpaidFine > MaxUnpaidFine)
                return (false,
                    $"Unpaid fine ₹{unpaidFine} exceeds limit ₹{MaxUnpaidFine}");

            // Step 3: Check borrowing limit
            var activeBorrows =
                _borrowRepo.GetActiveBorrowsByMemberId(memberId);

            if (activeBorrows.Count >= member.MembershipType.MaxBorrowings)
            {
                return (false,
                    $"Borrow limit reached. Maximum {member.MembershipType.MaxBorrowings} books allowed.");
            }

            // Step 4: Check duplicate borrow
            var duplicateBorrow =
                _borrowRepo.GetActiveBorrowByMemberAndBook(memberId, bookId);

            if (duplicateBorrow.Count > 0)
                return (false,
                    "Member already borrowed this book.");

            // Step 5: Check available copy
            var availableCopy = _bookRepo.GetAvailableCopy(bookId);

            if (availableCopy == null)
                return (false, "No copies available.");

            // Step 6: Create borrow record
            var borrow = new Borrow
            {
                MemberId = memberId,
                BookCopyId = availableCopy.Id,
                DateOfBorrow = DateOnly.FromDateTime(DateTime.Today),
                DueDate = DateOnly.FromDateTime(
                    DateTime.Today.AddDays(member.MembershipType.MaxBorrowDays)
                ),
                FineAmount = 0,
                Status = (int)BorrowStatus.Borrowed
            };

            _borrowRepo.AddBorrow(borrow);

            // Step 7: Update copy status
            availableCopy.Status = (int)CopyStatus.Borrowed;

            _bookRepo.UpdateBookCopy(availableCopy);

            transaction.Commit();

            return (true,
                $"Book borrowed successfully. Due Date: {borrow.DueDate}");
        }
        catch (Exception ex)
        {
            transaction.Rollback();

            return (false, $"Borrow failed: {ex.Message}");
        }
    }

    public (bool Success, string Message) ReturnBook(int borrowId)
    {
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            var borrow = _borrowRepo.GetAll()
                .FirstOrDefault(b => b.Id == borrowId);

            if (borrow == null)
                return (false, "Borrow record not found.");

            if (borrow.Status == (int)BorrowStatus.Returned)
                return (false, "Book already returned.");

            var today = DateOnly.FromDateTime(DateTime.Today);

            var overdueDays =
                today.DayNumber - borrow.DueDate.DayNumber;

            var fine =
                overdueDays > 0
                    ? overdueDays * FinePerDay
                    : 0;

            // Update borrow
            borrow.DateOfReturn = today;
            borrow.FineAmount = fine;
            borrow.Status = (int)BorrowStatus.Returned;

            _borrowRepo.UpdateBorrow(borrow);

            // Update copy status
            var copy = _bookRepo
                .GetCopiesByBookId(borrow.BookCopy.BookId)
                .FirstOrDefault(c => c.Id == borrow.BookCopyId);

            if (copy != null)
            {
                copy.Status = (int)CopyStatus.Available;

                _bookRepo.UpdateBookCopy(copy);
            }

            transaction.Commit();

            if (fine > 0)
            {
                return (true,
                    $"Book returned with fine ₹{fine}");
            }

            return (true, "Book returned successfully.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();

            return (false,
                $"Return failed: {ex.Message}");
        }
    }

    public List<BorrowDto> GetAllBorrows()
    {
        var borrows = _borrowRepo.GetAll();

        return borrows.Select(MapToDto).ToList();
    }

    public List<BorrowDto> GetBorrowsByMember(int memberId)
    {
        var borrows =
            _borrowRepo.GetBorrowsByMemberId(memberId);

        return borrows.Select(MapToDto).ToList();
    }

    public List<BorrowDto> GetActiveBorrows()
    {
        var borrows = _borrowRepo.GetAll();

        return borrows
            .Where(b => b.Status == (int)BorrowStatus.Borrowed)
            .Select(MapToDto)
            .ToList();
    }

    public List<BorrowDto> GetOverdueBorrows()
    {
        var borrows = _borrowRepo.GetOverdueBorrows();

        return borrows.Select(MapToDto).ToList();
    }

    public BorrowingSummaryDto? GetMemberBorrowingSummary(int memberId)
    {
        var member = _memberRepo.GetMemberById(memberId);

        if (member == null)
            return null;

        var borrows =
            _borrowRepo.GetBorrowsByMemberId(memberId);

        var active = borrows
            .Where(b => b.Status == (int)BorrowStatus.Borrowed)
            .ToList();

        var returned = borrows
            .Where(b => b.Status == (int)BorrowStatus.Returned)
            .ToList();

        var unpaidFine =
            _fineRepo.GetTotalUnpaidFine(memberId);

        return new BorrowingSummaryDto
        {
            MemberId = memberId,
            MemberName = member.Name,
            ActiveBorrowings = active.Count,
            ReturnedBorrowings = returned.Count,
            TotalUnpaidFine = unpaidFine ?? 0,
            ActiveBooks = active.Select(MapToDto).ToList()
        };
    }

    private static BorrowDto MapToDto(Borrow b)
    {
        return new BorrowDto
        {
            Id = b.Id,
            MemberId = b.MemberId,
            MemberName = b.Member?.Name ?? "-",
            BookCopyId = b.BookCopyId,
            BookTitle = b.BookCopy?.Book?.Title ?? "-",
            BookAuthor = b.BookCopy?.Book?.Author ?? "-",
            DateOfBorrow = b.DateOfBorrow,
            DueDate = b.DueDate,
            DateOfReturn = b.DateOfReturn,
            FineAmount = b.FineAmount,
            Status = (BorrowStatus)b.Status
        };
    }
}