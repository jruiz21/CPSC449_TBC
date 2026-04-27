using TheBorrowedChapter.Dtos;

namespace TheBorrowedChapter.Services;

public interface IBookService
{
    Task<IEnumerable<BookResponse>> GetAllBooksAsync();
    Task<BookResponse?> GetBookByIdAsync(Guid id);
    Task<BookResult> CreateBookAsync(CreateBookRequest request);
    Task<BookResult> UpdateBookAsync(Guid id, UpdateBookRequest request);
    Task<BookResult> DeleteBookAsync(Guid id);
}
