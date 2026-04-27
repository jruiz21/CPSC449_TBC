namespace TheBorrowedChapter.Models;

public enum BorrowStatus
{
    Borrowed,
    Returned
}

public class BorrowRecord
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid MemberId { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public BorrowStatus Status { get; set; } = BorrowStatus.Borrowed;

    public Book? Book { get; set; }
    public Member? Member { get; set; }
}
