using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces;
public interface IMemberRepository
{
    public List<Member> GetAllMembers();
    public Member? GetMemberById(int id);
    public List<Member> SearchMembers(string keyword);
    public void AddMember(Member member);
    public void UpdateMember(Member member);

}