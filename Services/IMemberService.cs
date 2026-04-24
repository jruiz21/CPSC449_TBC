using TheBorrowedChapter.Dtos;

namespace TheBorrowedChapter.Services;

public interface IMemberService
{
    Task<IEnumerable<MemberResponse>> GetAllAsync();
    Task<MemberResult> GetByIdAsync(Guid id);
    Task<MemberResult> CreateAsync(CreateMemberRequest request);
    Task<MemberResult> UpdateAsync(Guid id, UpdateMemberRequest request);
    Task<MemberResult> DeleteAsync(Guid id);
}
