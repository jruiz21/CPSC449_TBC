namespace TheBorrowedChapter.Dtos;

public class BorrowBookRequest
{
    public Guid BookId { get; set; }
    public Guid MemberId { get; set; }
}
