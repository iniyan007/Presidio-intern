using LibraryManagement.Models;

namespace LibraryManagement.Service;
public interface IMemberService
{
    public List<Member> GetAllMembers();
    public Member? GetMemberById(int id);
    public void AddMember(Member member);
}