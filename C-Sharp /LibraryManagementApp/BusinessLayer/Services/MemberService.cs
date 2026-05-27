using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using BusinessLayer.Interfaces;
using BusinessLayer.DTOs;
using DataAccessLayer.Enums;

namespace BusinessLayer.Services;
public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public List<MemberDto> GetAllMembers()
    {
        var members = _memberRepository.GetAllMembers();
        return members.Select(m => new MemberDto
        {
            Id = m.Id,
            Name = m.Name,
            Phone = m.Phone,
            Email = m.Email,
            MembershipType = (MembershipTypeEnum)m.MembershipTypeId,
            Status = (MemberStatus)m.Status
        }).ToList();
    }

    public MemberDto? GetMemberById(int id)
    {
        var member = _memberRepository.GetMemberById(id);
        if (member == null) return null;

        return new MemberDto
        {
            Id = member.Id,
            Name = member.Name,
            Phone = member.Phone,
            Email = member.Email,
            MembershipType = (MembershipTypeEnum)member.MembershipTypeId,
            Status = (MemberStatus)member.Status
        };
    }
    public List<MemberDto> GetMemberByKeyWord(string keyword)
    {
        var members = _memberRepository.SearchMembers(keyword);
        return members.Select(m => new MemberDto
        {
            Id = m.Id,
            Name = m.Name,
            Phone = m.Phone,    
            Email = m.Email,
            MembershipType = (MembershipTypeEnum)m.MembershipTypeId,
            Status = (MemberStatus)m.Status
        }).ToList();
    }
    public (bool Success, string Message) AddMember(string name, string phone, string email, MembershipTypeEnum membershipType)
    {
        var member = new Member
        {
            Name = name,
            Phone = phone,
            Email = email,
            MembershipTypeId = (int)membershipType,
            Status = (int)MemberStatus.Active
        };
        _memberRepository.AddMember(member);
        return (true, "Member added successfully.");
    }
    public (bool Success, string Message) UpdateMemberStatus(int id, MemberStatus status)
    {
        var member = _memberRepository.GetMemberById(id);
        if (member == null)
        {
            return (false, "Member not found.");
        }
        member.Status = (int)status;
        _memberRepository.UpdateMember(member);
        return (true, "Member status updated successfully.");
    }
    public (bool Success, string Message) UpdateMember(int id, string name, string phone, string email, MembershipTypeEnum membershipType)
    {
        var member = _memberRepository.GetMemberById(id);
        if (member == null)
        {
            return (false, "Member not found.");
        }

        member.Name = name;
        member.Phone = phone;
        member.Email = email;
        member.MembershipTypeId = (int)membershipType;
        _memberRepository.UpdateMember(member);
        return (true, "Member updated successfully.");
    }
    public (bool Success, string Message) DeactivateMember(int id)
    {
        var member = _memberRepository.GetMemberById(id);
        if (member == null)
        {
            return (false, "Member not found.");
        }
        member.Status = (int)MemberStatus.Inactive;
        _memberRepository.UpdateMember(member);
        return (true, "Member deactivated successfully.");
    }
}