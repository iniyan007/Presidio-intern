using BusinessLayer.DTOs;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Validators;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace BusinessLayer.Services;

public class BorrowService : IBorrowService
{
    private readonly IBorrowRepository _borrowRepo;
    private readonly IMemberRepository _memberRepo;
    private readonly IBookRepository   _bookRepo;
    private readonly IFineRepository   _fineRepo;
    private readonly LibraryDbContext  _context;

    private const decimal FinePerDay    = 10m;
    private const decimal MaxUnpaidFine = 500m;

    public BorrowService(
        IBorrowRepository borrowRepo,
        IMemberRepository memberRepo,
        IBookRepository   bookRepo,
        IFineRepository   fineRepo,
        LibraryDbContext  context)
    {
        _borrowRepo = borrowRepo;
        _memberRepo = memberRepo;
        _bookRepo   = bookRepo;
        _fineRepo   = fineRepo;
        _context    = context;
    }

    public async Task<(bool Success, string Message)> BorrowBookAsync(int memberId, int bookId)
    {
        InputValidator.ValidateId(memberId, "Member ID");
        InputValidator.ValidateId(bookId, "Book ID");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var member = await _memberRepo.GetByIdAsync(memberId)
                ?? throw new MemberNotFoundException(memberId);

            if (member.Status == (int)MemberStatus.Inactive)
                throw new InactiveMemberException(member.Name);

            var unpaidFine = await _fineRepo.GetTotalUnpaidFineAsync(memberId);
            if (unpaidFine > 500)
                throw new UnpaidFineException(unpaidFine);

            var activeBorrows = await _borrowRepo.GetActiveBorrowsByMemberIdAsync(memberId);
            if (activeBorrows.Count >= member.MembershipType.MaxBorrowings)
                throw new BorrowLimitExceededException(member.MembershipType.MaxBorrowings);

            var duplicate = await _borrowRepo.GetActiveBorrowByMemberAndBookAsync(memberId, bookId);
            if (duplicate is not null)
                throw new DuplicateBorrowException();

            var availableCopy = await _bookRepo.GetAvailableCopyAsync(bookId);
            if (availableCopy is null)
                throw new BookNotAvailableException(bookId);

            var borrow = new Borrow
            {
                MemberId     = memberId,
                BookCopyId   = availableCopy.Id,
                DateOfBorrow = DateOnly.FromDateTime(DateTime.Today),
                DueDate      = DateOnly.FromDateTime(DateTime.Today.AddDays(member.MembershipType.MaxBorrowDays)),
                FineAmount   = 0,
                Status       = (int)BorrowStatus.Borrowed
            };

            await _borrowRepo.AddAsync(borrow);

            availableCopy.Status = (int)CopyStatus.Borrowed;
            await _bookRepo.UpdateCopyAsync(availableCopy);

            await transaction.CommitAsync();
            return (true, $"Book borrowed successfully. Due date: {borrow.DueDate}");
        }
        catch (LibraryException)
        {
            await transaction.RollbackAsync();
            throw; 
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new LibraryException($"Unexpected error during borrowing: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ReturnBookAsync(int borrowId)
    {
        InputValidator.ValidateId(borrowId, "Borrow ID");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var borrow = await _borrowRepo.GetByIdAsync(borrowId)
                ?? throw new LibraryException($"Borrow record ID {borrowId} not found.");

            if (borrow.Status == (int)BorrowStatus.Returned)
                throw new AlreadyReturnedException(borrowId);

            var today       = DateOnly.FromDateTime(DateTime.Today);
            var overdueDays = today.DayNumber - borrow.DueDate.DayNumber;
            var fine        = overdueDays > 0 ? overdueDays * 10m : 0;

            borrow.DateOfReturn = today;
            borrow.FineAmount   = fine;
            borrow.Status       = (int)BorrowStatus.Returned;
            await _borrowRepo.UpdateAsync(borrow);

            borrow.BookCopy.Status = (int)CopyStatus.Available;
            await _bookRepo.UpdateCopyAsync(borrow.BookCopy);

            await transaction.CommitAsync();

            return fine > 0
                ? (true, $"Book returned. Late by {overdueDays} day(s). Fine: ₹{fine}")
                : (true, "Book returned successfully. No fine.");
        }
        catch (LibraryException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new LibraryException($"Unexpected error during return: {ex.Message}");
        }
    }

    public async Task<List<BorrowDto>> GetAllBorrowsAsync()
    {
        var borrows = await _borrowRepo.GetAllAsync();
        return borrows.Select(MapToDto).ToList();
    }

    public async Task<List<BorrowDto>> GetBorrowsByMemberAsync(int memberId)
    {
        var borrows = await _borrowRepo.GetByMemberIdAsync(memberId);
        return borrows.Select(MapToDto).ToList();
    }

    public async Task<List<BorrowDto>> GetActiveBorrowsAsync()
    {
        var borrows = await _borrowRepo.GetAllAsync();
        return borrows.Where(b => b.Status == (int)BorrowStatus.Borrowed)
                      .Select(MapToDto).ToList();
    }

    public async Task<List<BorrowDto>> GetOverdueBorrowsAsync()
    {
        var borrows = await _borrowRepo.GetOverdueBorrowsAsync();
        return borrows.Select(MapToDto).ToList();
    }
    public async Task<List<BorrowDto>> GetActiveBorrowsByMemberAsync(int memberId)
    {
        InputValidator.ValidateId(memberId, "Member ID");

        var member = await _memberRepo.GetByIdAsync(memberId)
            ?? throw new MemberNotFoundException(memberId);

        var borrows = await _borrowRepo.GetActiveBorrowsByMemberIdAsync(memberId);
        return borrows.Select(MapToDto).ToList();
    }
    public async Task<BorrowingSummaryDto?> GetMemberBorrowingSummaryAsync(int memberId)
    {
        var member = await _memberRepo.GetByIdAsync(memberId);
        if (member is null) return null;

        var borrows     = await _borrowRepo.GetByMemberIdAsync(memberId);
        var active      = borrows.Where(b => b.Status == (int)BorrowStatus.Borrowed).ToList();
        var returned    = borrows.Where(b => b.Status == (int)BorrowStatus.Returned).ToList();
        var unpaidFine  = await _fineRepo.GetTotalUnpaidFineAsync(memberId);

        return new BorrowingSummaryDto
        {
            MemberId            = memberId,
            MemberName          = member.Name,
            ActiveBorrowings    = active.Count,
            ReturnedBorrowings  = returned.Count,
            TotalUnpaidFine     = unpaidFine,
            ActiveBooks         = active.Select(MapToDto).ToList()
        };
    }

    // ── Mapper ────────────────────────────────────────────────
    private static BorrowDto MapToDto(Borrow b) => new()
    {
        Id           = b.Id,
        MemberId     = b.MemberId,
        MemberName   = b.Member?.Name ?? "-",
        BookCopyId   = b.BookCopyId,
        BookTitle    = b.BookCopy?.Book?.Title ?? "-",
        BookAuthor   = b.BookCopy?.Book?.Author ?? "-",
        CategoryName = b.BookCopy?.Book?.Category?.Name ?? "-",
        CopyRemarks  = b.BookCopy?.Remarks ?? "No remarks",   
        DateOfBorrow = b.DateOfBorrow,
        DueDate      = b.DueDate,
        DateOfReturn = b.DateOfReturn,
        FineAmount   = b.FineAmount,
        Status       = (BorrowStatus)b.Status
    };
}