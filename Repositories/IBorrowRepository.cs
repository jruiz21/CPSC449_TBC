using TheBorrowedChapter.Models;

namespace TheBorrowedChapter.Repositories;

public interface IBorrowRepository
{
    Task<IEnumerable<BorrowRecord>> GetAllAsync();
    Task<IEnumerable<BorrowRecord>> GetByMemberIdAsync(Guid memberId);
    Task<bool> HasActiveBorrowAsync(Guid bookId, Guid memberId);
    Task<BorrowRepositoryResult> BorrowBookAsync(Guid bookId, Guid memberId, DateTime borrowDate);
    Task<BorrowRepositoryResult> ReturnBookAsync(Guid bookId, Guid memberId, DateTime returnDate);
}

public enum BorrowRepositoryStatus
{
    Success,
    NoAvailableCopies,
    AlreadyBorrowed,
    NotBorrowed
}

public class BorrowRepositoryResult
{
    public BorrowRepositoryStatus Status { get; init; }
    public BorrowRecord? Record { get; init; }
}
