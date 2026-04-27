using Microsoft.EntityFrameworkCore;
using TheBorrowedChapter.Data;
using TheBorrowedChapter.Models;

namespace TheBorrowedChapter.Repositories;

public class BorrowRepository : IBorrowRepository
{
    private readonly ApplicationDbContext _context;

    public BorrowRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BorrowRecord>> GetAllAsync()
    {
        return await _context.BorrowRecords
            .AsNoTracking()
            .OrderByDescending(r => r.BorrowDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BorrowRecord>> GetByMemberIdAsync(Guid memberId)
    {
        return await _context.BorrowRecords
            .AsNoTracking()
            .Where(r => r.MemberId == memberId)
            .OrderByDescending(r => r.BorrowDate)
            .ToListAsync();
    }

    public async Task DeleteByBookIdAsync(Guid bookId)
    {
        await _context.BorrowRecords
            .Where(r => r.BookId == bookId)
            .ExecuteDeleteAsync();
    }

    public async Task DeleteByMemberIdAsync(Guid memberId)
    {
        await _context.BorrowRecords
            .Where(r => r.MemberId == memberId)
            .ExecuteDeleteAsync();
    }

    public async Task<BorrowRepositoryResult> BorrowBookAsync(Guid bookId, Guid memberId, DateTime borrowDate)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var alreadyBorrowed = await _context.BorrowRecords.AnyAsync(r =>
            r.BookId == bookId &&
            r.MemberId == memberId &&
            r.Status == BorrowStatus.Borrowed);

        if (alreadyBorrowed)
        {
            await transaction.RollbackAsync();
            return new BorrowRepositoryResult { Status = BorrowRepositoryStatus.AlreadyBorrowed };
        }

        var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Books
            SET AvailableCopies = AvailableCopies - 1
            WHERE Id = {bookId} AND AvailableCopies > 0");

        if (rowsAffected == 0)
        {
            await transaction.RollbackAsync();
            return new BorrowRepositoryResult { Status = BorrowRepositoryStatus.NoAvailableCopies };
        }

        var record = new BorrowRecord
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = borrowDate,
            Status = BorrowStatus.Borrowed
        };

        _context.BorrowRecords.Add(record);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new BorrowRepositoryResult
        {
            Status = BorrowRepositoryStatus.Success,
            Record = record
        };
    }

    public async Task<BorrowRepositoryResult> ReturnBookAsync(Guid bookId, Guid memberId, DateTime returnDate)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var record = await _context.BorrowRecords.FirstOrDefaultAsync(r =>
            r.BookId == bookId &&
            r.MemberId == memberId &&
            r.Status == BorrowStatus.Borrowed);

        if (record is null)
        {
            await transaction.RollbackAsync();
            return new BorrowRepositoryResult { Status = BorrowRepositoryStatus.NotBorrowed };
        }

        record.Status = BorrowStatus.Returned;
        record.ReturnDate = returnDate;

        var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Books
            SET AvailableCopies = AvailableCopies + 1
            WHERE Id = {bookId}");

        if (rowsAffected == 0)
        {
            await transaction.RollbackAsync();
            return new BorrowRepositoryResult { Status = BorrowRepositoryStatus.NotBorrowed };
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new BorrowRepositoryResult
        {
            Status = BorrowRepositoryStatus.Success,
            Record = record
        };
    }
}
