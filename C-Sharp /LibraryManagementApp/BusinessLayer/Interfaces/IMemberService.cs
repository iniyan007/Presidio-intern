using BusinessLayer.DTOs;
using DataAccessLayer.Enums;

namespace BusinessLayer.Interfaces;
public interface IMemberService
{
    List<MemberDto> GetAllMembers();
    MemberDto? GetMemberById(int id);
    List<MemberDto> GetMemberByKeyWord(string keyword);
    (bool Success, string Message) AddMember(string name, string phone, string email, MembershipTypeEnum membershipType);
    (bool Success, string Message) UpdateMember(int id, string name, string phone, string email, MembershipTypeEnum membershipType);
    (bool Success, string Message) UpdateMemberStatus(int id, MemberStatus status);
    (bool Success, string Message) DeactivateMember(int id);
}