using BusinessLayer.DTOs;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Validators;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;

namespace BusinessLayer.Services;

public class FineService : IFineService
{
    private readonly IFineRepository   _fineRepo;
    private readonly IMemberRepository _memberRepo;

    public FineService(IFineRepository fineRepo, IMemberRepository memberRepo)
    {
        _fineRepo   = fineRepo;
        _memberRepo = memberRepo;
    }

    public async Task<decimal> GetUnpaidFineAsync(int memberId)
    {
        return await _fineRepo.GetTotalUnpaidFineAsync(memberId);
    }

    public async Task<List<FinePaymentDto>> GetFineHistoryAsync(int memberId)
    {
        var payments = await _fineRepo.GetByMemberIdAsync(memberId);
        return payments.Select(p => new FinePaymentDto
        {
            Id          = p.Id,
            BorrowId    = p.BorrowId,
            BookTitle   = p.Borrow?.BookCopy?.Book?.Title ?? "-",
            AmountPaid  = p.AmountPaid,
            PaymentDate = p.PaymentDate
        }).ToList();
    }

    public async Task<(bool Success, string Message)> PayFineAsync(int memberId, decimal amount)
    {
        InputValidator.ValidateId(memberId, "Member ID");
        InputValidator.ValidateAmount(amount);

        var member = await _memberRepo.GetByIdAsync(memberId)
            ?? throw new MemberNotFoundException(memberId);

        var unpaidFine = await _fineRepo.GetTotalUnpaidFineAsync(memberId);

        if (unpaidFine <= 0)
            throw new LibraryException("No pending fines for this member.");

        if (amount > unpaidFine)
            throw new LibraryException($"Amount ₹{amount} exceeds unpaid fine of ₹{unpaidFine}.");

        var payment = new FinePayment
        {
            MemberId    = memberId,
            BorrowId    = 1,
            AmountPaid  = amount,
            PaymentDate = DateTime.Now
        };

        await _fineRepo.AddPaymentAsync(payment);
        return (true, $"Payment of ₹{amount} recorded. Remaining fine: ₹{unpaidFine - amount}");
    }
    public async Task<FineDto> GetFineSummaryAsync(int memberId)
    {
        var member     = await _memberRepo.GetByIdAsync(memberId);
        var unpaid     = await _fineRepo.GetTotalUnpaidFineAsync(memberId);
        var paid       = await _fineRepo.GetTotalPaidFineAsync(memberId);

        return new FineDto
        {
            MemberId   = memberId,
            MemberName = member?.Name ?? "-",
            TotalFine  = unpaid + paid,
            TotalPaid  = paid,
            UnpaidFine = unpaid
        };
    }
    public async Task<(bool Success, string Message)> PayFineForBorrowAsync(
        int memberId, int borrowId, decimal amount)
    {
        InputValidator.ValidateId(memberId, "Member ID");
        InputValidator.ValidateId(borrowId, "Borrow ID");
        InputValidator.ValidateAmount(amount);

        var member = await _memberRepo.GetByIdAsync(memberId)
            ?? throw new MemberNotFoundException(memberId);

        var payment = new FinePayment
        {
            MemberId    = memberId,
            BorrowId    = borrowId,
            AmountPaid  = amount,
            PaymentDate = DateTime.Now
        };

        await _fineRepo.AddPaymentAsync(payment);
        return (true, $"Fine of ₹{amount} recorded for Member '{member.Name}'.");
    }
}