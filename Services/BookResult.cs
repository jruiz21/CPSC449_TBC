using TheBorrowedChapter.Dtos;

namespace TheBorrowedChapter.Services;

public enum BookErrorType { None, NotFound, DuplicateISBN, InvalidData }

public class BookResult
{
    public BookResponse? Book { get; private set; }
    public BookErrorType ErrorType { get; private set; }
    public string? ErrorMessage { get; private set; }

    public bool IsSuccess => ErrorType == BookErrorType.None;

    public static BookResult Success(BookResponse book) =>
        new() { Book = book, ErrorType = BookErrorType.None };

    public static BookResult Failure(BookErrorType errorType, string message) =>
        new() { ErrorType = errorType, ErrorMessage = message };
}
