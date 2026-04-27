using Microsoft.Extensions.Caching.Memory;
using TheBorrowedChapter.Dtos;
using TheBorrowedChapter.Models;
using TheBorrowedChapter.Repositories;

namespace TheBorrowedChapter.Services;

public class BorrowService : IBorrowService
{
    private readonly IBorrowRepository _borrowRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemoryCache _cache;

    public BorrowService(
        IBorrowRepository borrowRepository,
        IBookRepository bookRepository,
        IMemberRepository memberRepository,
        IMemoryCache cache)
    {
        _borrowRepository = borrowRepository;
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
        _cache = cache;
    }

    public async Task<IEnumerable<BorrowRecordResponse>> GetAllAsync()
    {
        var records = await _borrowRepository.GetAllAsync();
        return records.Select(ToResponse);
    }

    public async Task<IEnumerable<BorrowRecordResponse>> GetMemberHistoryAsync(Guid memberId)
    {
        if (memberId == Guid.Empty)
            return [];

        var records = await _borrowRepository.GetByMemberIdAsync(memberId);
        return records.Select(ToResponse);
    }

    public async Task<BorrowResult> BorrowBookAsync(BorrowBookRequest request)
    {
        if (request.BookId == Guid.Empty)
            return BorrowResult.Failure(BorrowErrorType.InvalidData, "BookId is required.");
        if (request.MemberId == Guid.Empty)
            return BorrowResult.Failure(BorrowErrorType.InvalidData, "MemberId is required.");

        var bookExists = await _bookRepository.ExistsAsync(request.BookId);
        if (!bookExists)
            return BorrowResult.Failure(BorrowErrorType.NotFound, $"Book with id {request.BookId} was not found.");

        var memberExists = await _memberRepository.ExistsAsync(request.MemberId);
        if (!memberExists)
            return BorrowResult.Failure(BorrowErrorType.NotFound, $"Member with id {request.MemberId} was not found.");

        var result = await _borrowRepository.BorrowBookAsync(request.BookId, request.MemberId, DateTime.UtcNow);

        if (result.Status == BorrowRepositoryStatus.AlreadyBorrowed)
        {
            return BorrowResult.Failure(
                BorrowErrorType.AlreadyBorrowed,
                "This member already has an active borrow record for this book.");
        }

        if (result.Status == BorrowRepositoryStatus.NoAvailableCopies)
        {
            return BorrowResult.Failure(
                BorrowErrorType.NoAvailableCopies,
                "This book has no available copies to borrow.");
        }

        InvalidateBookCache(request.BookId);
        return BorrowResult.Success(ToResponse(result.Record!));
    }

    public async Task<BorrowResult> ReturnBookAsync(ReturnBookRequest request)
    {
        if (request.BookId == Guid.Empty)
            return BorrowResult.Failure(BorrowErrorType.InvalidData, "BookId is required.");
        if (request.MemberId == Guid.Empty)
            return BorrowResult.Failure(BorrowErrorType.InvalidData, "MemberId is required.");

        var bookExists = await _bookRepository.ExistsAsync(request.BookId);
        if (!bookExists)
            return BorrowResult.Failure(BorrowErrorType.NotFound, $"Book with id {request.BookId} was not found.");

        var memberExists = await _memberRepository.ExistsAsync(request.MemberId);
        if (!memberExists)
            return BorrowResult.Failure(BorrowErrorType.NotFound, $"Member with id {request.MemberId} was not found.");

        var result = await _borrowRepository.ReturnBookAsync(request.BookId, request.MemberId, DateTime.UtcNow);

        if (result.Status == BorrowRepositoryStatus.NotBorrowed)
        {
            return BorrowResult.Failure(
                BorrowErrorType.NotBorrowed,
                "This member does not have an active borrow record for this book.");
        }

        InvalidateBookCache(request.BookId);
        return BorrowResult.Success(ToResponse(result.Record!));
    }

    private void InvalidateBookCache(Guid bookId)
    {
        _cache.Remove(BookCacheKeys.AllBooks);
        _cache.Remove(BookCacheKeys.BookById(bookId));
    }

    private static BorrowRecordResponse ToResponse(BorrowRecord record) => new()
    {
        Id = record.Id,
        BookId = record.BookId,
        MemberId = record.MemberId,
        BorrowDate = record.BorrowDate,
        ReturnDate = record.ReturnDate,
        Status = record.Status
    };
}
