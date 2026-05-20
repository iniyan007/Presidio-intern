using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;

namespace LibraryManagement.Repository;
public class MemberRepository : IMemberRepository
{
    private readonly LibraryDbContext _context;

    public MemberRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public List<Member> GetAllMembers()
    {
        return _context.Members.ToList();
    }

    public Member? GetMemberById(int id)
    {
        return _context.Members.FirstOrDefault(m => m.MemberId == id);
    }

    public void AddMember(Member member)
    {
        _context.Members.Add(member);
        _context.SaveChanges();
    }
}