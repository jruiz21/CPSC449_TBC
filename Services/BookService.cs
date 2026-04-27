using Microsoft.Extensions.Caching.Memory;
using TheBorrowedChapter.Dtos;
using TheBorrowedChapter.Models;
using TheBorrowedChapter.Repositories;

namespace TheBorrowedChapter.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBorrowRepository _borrowRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BookService> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public BookService(
        IBookRepository bookRepository,
        IBorrowRepository borrowRepository,
        IMemoryCache cache,
        ILogger<BookService> logger)
    {
        _bookRepository = bookRepository;
        _borrowRepository = borrowRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<BookResponse>> GetAllBooksAsync()
    {
        if (_cache.TryGetValue(BookCacheKeys.AllBooks, out IEnumerable<BookResponse>? cached) && cached is not null)
        {
            _logger.LogInformation("Cache hit for all books.");
            return cached;
        }

        _logger.LogInformation("Cache miss for all books. Loading from database.");

        var books = await _bookRepository.GetAllAsync();
        var response = books.Select(ToResponse).ToList();

        _cache.Set(BookCacheKeys.AllBooks, response, CacheDuration);
        return response;
    }

    public async Task<BookResponse?> GetBookByIdAsync(Guid id)
    {
        var cacheKey = BookCacheKeys.BookById(id);
        if (_cache.TryGetValue(cacheKey, out BookResponse? cached) && cached is not null)
        {
            _logger.LogInformation("Cache hit for book {BookId}.", id);
            return cached;
        }

        _logger.LogInformation("Cache miss for book {BookId}. Loading from database.", id);

        var book = await _bookRepository.GetByIdAsync(id);
        if (book is null)
            return null;

        var response = ToResponse(book);
        _cache.Set(cacheKey, response, CacheDuration);
        return response;
    }

    public async Task<BookResult> CreateBookAsync(CreateBookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BookResult.Failure(BookErrorType.InvalidData, "Title is required.");
        if (string.IsNullOrWhiteSpace(request.Author))
            return BookResult.Failure(BookErrorType.InvalidData, "Author is required.");
        if (string.IsNullOrWhiteSpace(request.ISBN))
            return BookResult.Failure(BookErrorType.InvalidData, "ISBN is required.");
        if (request.TotalCopies <= 0)
            return BookResult.Failure(BookErrorType.InvalidData, "TotalCopies must be greater than 0.");
        if (request.AvailableCopies < 0)
            return BookResult.Failure(BookErrorType.InvalidData, "AvailableCopies must be greater than or equal to 0.");
        if (request.AvailableCopies > request.TotalCopies)
            return BookResult.Failure(BookErrorType.InvalidData, "AvailableCopies cannot exceed TotalCopies.");

        if (await _bookRepository.IsbnExistsAsync(request.ISBN))
            return BookResult.Failure(BookErrorType.DuplicateISBN, $"A book with ISBN '{request.ISBN}' already exists.");

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Author = request.Author,
            ISBN = request.ISBN,
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.AvailableCopies
        };

        var created = await _bookRepository.AddAsync(book);
        InvalidateCache(created.Id);
        return BookResult.Success(ToResponse(created));
    }

    public async Task<BookResult> UpdateBookAsync(Guid id, UpdateBookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BookResult.Failure(BookErrorType.InvalidData, "Title is required.");
        if (string.IsNullOrWhiteSpace(request.Author))
            return BookResult.Failure(BookErrorType.InvalidData, "Author is required.");
        if (string.IsNullOrWhiteSpace(request.ISBN))
            return BookResult.Failure(BookErrorType.InvalidData, "ISBN is required.");
        if (request.TotalCopies <= 0)
            return BookResult.Failure(BookErrorType.InvalidData, "TotalCopies must be greater than 0.");
        if (request.AvailableCopies < 0)
            return BookResult.Failure(BookErrorType.InvalidData, "AvailableCopies must be greater than or equal to 0.");
        if (request.AvailableCopies > request.TotalCopies)
            return BookResult.Failure(BookErrorType.InvalidData, "AvailableCopies cannot exceed TotalCopies.");

        var book = await _bookRepository.GetByIdAsync(id);
        if (book is null)
            return BookResult.Failure(BookErrorType.NotFound, $"Book with id {id} was not found.");

        if (await _bookRepository.IsbnExistsAsync(request.ISBN, excludeId: id))
            return BookResult.Failure(BookErrorType.DuplicateISBN, $"A book with ISBN '{request.ISBN}' already exists.");

        book.Title = request.Title;
        book.Author = request.Author;
        book.ISBN = request.ISBN;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = request.AvailableCopies;

        var updated = await _bookRepository.UpdateAsync(book);
        InvalidateCache(updated.Id);
        return BookResult.Success(ToResponse(updated));
    }

    public async Task<BookResult> DeleteBookAsync(Guid id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book is null)
            return BookResult.Failure(BookErrorType.NotFound, $"Book with id {id} was not found.");

        await _borrowRepository.DeleteByBookIdAsync(id);
        await _bookRepository.DeleteAsync(book);
        InvalidateCache(id);
        return BookResult.Success(ToResponse(book));
    }

    private void InvalidateCache(Guid id)
    {
        _cache.Remove(BookCacheKeys.AllBooks);
        _cache.Remove(BookCacheKeys.BookById(id));
    }

    private static BookResponse ToResponse(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Author = book.Author,
        ISBN = book.ISBN,
        TotalCopies = book.TotalCopies,
        AvailableCopies = book.AvailableCopies
    };
}
