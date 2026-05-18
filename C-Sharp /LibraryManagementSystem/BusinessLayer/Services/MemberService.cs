using BusinessLayer.DTOs;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Validators;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;

namespace BusinessLayer.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepo;

    public MemberService(IMemberRepository memberRepo, IBookRepository bookRepo)
    {
        _memberRepo = memberRepo;
    }

    public async Task<List<MemberDto>> GetAllMembersAsync()
    {
        var members = await _memberRepo.GetAllAsync();
        return members.Select(MapToDto).ToList();
    }

    public async Task<MemberDto?> GetMemberByIdAsync(int id)
    {
        InputValidator.ValidateId(id, "Member ID");
        var member = await _memberRepo.GetByIdAsync(id);
        return member is null ? null : MapToDto(member);
    }

    public async Task<MemberDto?> GetMemberByEmailAsync(string email)
    {
        InputValidator.ValidateEmail(email);
        var member = await _memberRepo.GetByEmailAsync(email);
        return member is null ? null : MapToDto(member);
    }

    public async Task<MemberDto?> GetMemberByPhoneAsync(string phone)
    {
        InputValidator.ValidatePhone(phone);
        var member = await _memberRepo.GetByPhoneAsync(phone);
        return member is null ? null : MapToDto(member);
    }

    public async Task<(bool Success, string Message, int MemberId)> AddMemberAsync(
        string name, string phone, string email, MembershipTypeEnum membershipType)
    {
        InputValidator.ValidateName(name);
        InputValidator.ValidatePhone(phone);
        InputValidator.ValidateEmail(email);

        // Check duplicates
        if (await _memberRepo.GetByEmailAsync(email) is not null)
            throw new LibraryException("A member with this email already exists.");

        if (await _memberRepo.GetByPhoneAsync(phone) is not null)
            throw new LibraryException("A member with this phone number already exists.");

        var member = new Member
        {
            Name             = name.Trim(),
            Phone            = phone.Trim(),
            Email            = email.Trim(),
            MembershipTypeId = (int)membershipType + 1,
            Status           = (int)MemberStatus.Active,
            JoinedDate       = DateOnly.FromDateTime(DateTime.Today)
        };
        await _memberRepo.AddAsync(member);
        return (true, $"Member '{name}' added successfully.", member.Id);
    }

    public async Task<(bool Success, string Message)> UpdateMemberStatusAsync(int id, MemberStatus status)
    {
        InputValidator.ValidateId(id, "Member ID");

        var member = await _memberRepo.GetByIdAsync(id)
            ?? throw new MemberNotFoundException(id);

        member.Status = (int)status;
        await _memberRepo.UpdateAsync(member);
        return (true, $"Member status updated to {status}.");
    }

    public async Task<(bool Success, string Message)> DeactivateMemberAsync(int id)
    {
        return await UpdateMemberStatusAsync(id, MemberStatus.Inactive);
    }

    private static MemberDto MapToDto(Member m) => new()
    {
        Id             = m.Id,
        Name           = m.Name,
        Phone          = m.Phone,
        Email          = m.Email,
        MembershipType = (MembershipTypeEnum)m.MembershipType.Type,
        Status         = (MemberStatus)m.Status,
        JoinedDate     = m.JoinedDate,
        MaxBorrowings  = m.MembershipType.MaxBorrowings,
        MaxBorrowDays  = m.MembershipType.MaxBorrowDays
    };
}