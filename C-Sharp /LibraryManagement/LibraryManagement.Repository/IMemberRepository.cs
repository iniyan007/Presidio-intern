using LibraryManagement.Models;

namespace LibraryManagement.Repository;

public interface IMemberRepository
{
    public List<Member> GetAllMembers();
    public Member? GetMemberById(int id);
    public void AddMember(Member member);
}