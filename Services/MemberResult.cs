using TheBorrowedChapter.Dtos;

namespace TheBorrowedChapter.Services;

public enum MemberErrorType { None, NotFound, DuplicateEmail, InvalidData, HasBorrowHistory }

public class MemberResult
{
    public MemberResponse? Member { get; private set; }
    public MemberErrorType ErrorType { get; private set; }
    public string? ErrorMessage { get; private set; }

    public bool IsSuccess => ErrorType == MemberErrorType.None;

    public static MemberResult Success(MemberResponse member) =>
        new() { Member = member, ErrorType = MemberErrorType.None };

    public static MemberResult Failure(MemberErrorType errorType, string message) =>
        new() { ErrorType = errorType, ErrorMessage = message };
}
