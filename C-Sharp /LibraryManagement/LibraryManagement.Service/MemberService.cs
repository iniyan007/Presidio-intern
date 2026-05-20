using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Repository;
using LibraryManagement.Service.Exceptions; 

namespace LibraryManagement.Service;
public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public List<Member> GetAllMembers()
    {
        return _memberRepository.GetAllMembers();
    }

    public Member? GetMemberById(int id)
    {
        return _memberRepository.GetMemberById(id);
    }

    public void AddMember(Member member)
    {
        if(string.IsNullOrEmpty(member.FullName))
        {
            throw new FieldEmptyException("Name cannot be empty");
        }
        if(string.IsNullOrEmpty(member.Email))
        {
            throw new FieldEmptyException("Email cannot be empty");
        }
        if(string.IsNullOrEmpty(member.PhoneNumber))
        {
            throw new FieldEmptyException("Phone Number cannot be empty");
        }
        _memberRepository.AddMember(member);
    }
}