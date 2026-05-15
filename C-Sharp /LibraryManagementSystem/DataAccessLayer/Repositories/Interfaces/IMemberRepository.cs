using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IMemberRepository
{
    Task<List<Member>> GetAllAsync();
    Task<Member?> GetByIdAsync(int id);
    Task<Member?> GetByEmailAsync(string email);
    Task<Member?> GetByPhoneAsync(string phone);
    Task AddAsync(Member member);
    Task UpdateAsync(Member member);
    Task<bool> ExistsAsync(int id);
}