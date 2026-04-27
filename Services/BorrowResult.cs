using TheBorrowedChapter.Dtos;

namespace TheBorrowedChapter.Services;

public enum BorrowErrorType
{
    None,
    NotFound,
    InvalidData,
    NoAvailableCopies,
    AlreadyBorrowed,
    NotBorrowed
}

public class BorrowResult
{
    public BorrowRecordResponse? Record { get; private set; }
    public BorrowErrorType ErrorType { get; private set; }
    public string? ErrorMessage { get; private set; }

    public bool IsSuccess => ErrorType == BorrowErrorType.None;

    public static BorrowResult Success(BorrowRecordResponse record) =>
        new() { Record = record, ErrorType = BorrowErrorType.None };

    public static BorrowResult Failure(BorrowErrorType errorType, string message) =>
        new() { ErrorType = errorType, ErrorMessage = message };
}
