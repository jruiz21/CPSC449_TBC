using TheBorrowedChapter.Models;

namespace TheBorrowedChapter.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(Guid id);
    Task<Book> AddAsync(Book book);
    Task<Book> UpdateAsync(Book book);
    Task DeleteAsync(Book book);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> IsbnExistsAsync(string isbn, Guid? excludeId = null);
}
