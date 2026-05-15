using BusinessLayer.DTOs;
using DataAccessLayer.Enums;

namespace BusinessLayer.Interfaces;

public interface IMemberService
{
    Task<List<MemberDto>> GetAllMembersAsync();
    Task<MemberDto?> GetMemberByIdAsync(int id);
    Task<MemberDto?> GetMemberByEmailAsync(string email);
    Task<MemberDto?> GetMemberByPhoneAsync(string phone);
    Task<(bool Success, string Message)> AddMemberAsync(string name, string phone, string email, MembershipTypeEnum membershipType);
    Task<(bool Success, string Message)> UpdateMemberStatusAsync(int id, MemberStatus status);
    Task<(bool Success, string Message)> DeactivateMemberAsync(int id);
}