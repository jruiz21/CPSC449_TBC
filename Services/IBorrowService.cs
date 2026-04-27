using TheBorrowedChapter.Dtos;

namespace TheBorrowedChapter.Services;

public interface IBorrowService
{
    Task<IEnumerable<BorrowRecordResponse>> GetAllAsync();
    Task<IEnumerable<BorrowRecordResponse>> GetMemberHistoryAsync(Guid memberId);
    Task<BorrowResult> BorrowBookAsync(BorrowBookRequest request);
    Task<BorrowResult> ReturnBookAsync(ReturnBookRequest request);
}
