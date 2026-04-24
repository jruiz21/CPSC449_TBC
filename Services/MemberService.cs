using TheBorrowedChapter.Dtos;
using TheBorrowedChapter.Models;
using TheBorrowedChapter.Repositories;

namespace TheBorrowedChapter.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _repository;

    public MemberService(IMemberRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MemberResponse>> GetAllAsync()
    {
        var members = await _repository.GetAllAsync();
        return members.Select(m => new MemberResponse
        {
            Id = m.Id,
            FullName = m.FullName,
            Email = m.Email,
            MembershipDate = m.MembershipDate
        });
    }

    public async Task<MemberResult> GetByIdAsync(Guid id)
    {
        var member = await _repository.GetByIdAsync(id);
        if (member == null)
            return MemberResult.Failure(MemberErrorType.NotFound, "Member not found");

        return MemberResult.Success(new MemberResponse
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            MembershipDate = member.MembershipDate
        });
    }

    public async Task<MemberResult> CreateAsync(CreateMemberRequest request)
    {
        if (await _repository.EmailExistsAsync(request.Email))
            return MemberResult.Failure(MemberErrorType.DuplicateEmail, "A member with this email already exists.");

        var member = new Member
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            MembershipDate = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(member);

        return MemberResult.Success(new MemberResponse
        {
            Id = created.Id,
            FullName = created.FullName,
            Email = created.Email,
            MembershipDate = created.MembershipDate
        });
    }

    public async Task<MemberResult> UpdateAsync(Guid id, UpdateMemberRequest request)
    {
        var member = await _repository.GetByIdAsync(id);
        if (member == null)
            return MemberResult.Failure(MemberErrorType.NotFound, "Member not found");

        if (await _repository.EmailExistsAsync(request.Email, id))
            return MemberResult.Failure(MemberErrorType.DuplicateEmail, "A member with this email already exists.");

        member.FullName = request.FullName;
        member.Email = request.Email;

        var updated = await _repository.UpdateAsync(member);

        return MemberResult.Success(new MemberResponse
        {
            Id = updated.Id,
            FullName = updated.FullName,
            Email = updated.Email,
            MembershipDate = updated.MembershipDate
        });
    }

    public async Task<MemberResult> DeleteAsync(Guid id)
    {
        var member = await _repository.GetByIdAsync(id);
        if (member == null)
            return MemberResult.Failure(MemberErrorType.NotFound, "Member not found");

        await _repository.DeleteAsync(member);
        return MemberResult.Success(new MemberResponse
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            MembershipDate = member.MembershipDate
        });
    }
}