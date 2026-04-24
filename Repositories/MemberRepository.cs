using Microsoft.EntityFrameworkCore;
using TheBorrowedChapter.Data;
using TheBorrowedChapter.Models;

namespace TheBorrowedChapter.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly ApplicationDbContext _context;

    public MemberRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Member>> GetAllAsync()
    {
        return await _context.Members.AsNoTracking().ToListAsync();
    }

    public async Task<Member?> GetByIdAsync(Guid id)
    {
        return await _context.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Member> AddAsync(Member member)
    {
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<Member> UpdateAsync(Member member)
    {
        _context.Members.Update(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task DeleteAsync(Member member)
    {
        _context.Members.Remove(member);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        return _context.Members.AnyAsync(m => m.Id == id);
    }

    public Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
    {
        return _context.Members.AnyAsync(m => m.Email == email && (excludeId == null || m.Id != excludeId.Value));
    }
}