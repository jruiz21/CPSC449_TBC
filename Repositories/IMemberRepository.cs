using TheBorrowedChapter.Models;

namespace TheBorrowedChapter.Repositories;

public interface IMemberRepository
{
    Task<IEnumerable<Member>> GetAllAsync();
    Task<Member?> GetByIdAsync(Guid id);
    Task<Member> AddAsync(Member member);
    Task<Member> UpdateAsync(Member member);
    Task DeleteAsync(Member member);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);
}