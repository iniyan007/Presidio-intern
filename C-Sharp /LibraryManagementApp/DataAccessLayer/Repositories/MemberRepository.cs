using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Repositories;
public class MemberRepository : IMemberRepository
{
    private readonly AppDbContext _context;

    public MemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Member> GetAllMembers()
    {
        return _context.Members.ToList();
    }

    public Member? GetMemberById(int id)
    {
        return _context.Members.Find(id);
    }

    public List<Member> SearchMembers(string keyword)
    {
        return _context.Members
            .Where(m => m.Name.Contains(keyword) || m.Email.Contains(keyword) || m.Phone.Contains(keyword))
            .ToList();
    }

    public void AddMember(Member member)
    {
        _context.Members.Add(member);
        _context.SaveChanges();
    }

    public void UpdateMember(Member member)
    {
        _context.Members.Update(member);
        _context.SaveChanges();
    }
}