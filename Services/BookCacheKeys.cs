namespace TheBorrowedChapter.Services;

public static class BookCacheKeys
{
    public const string AllBooks = "books_all";
    public static string BookById(Guid id) => $"book_{id}";
}
