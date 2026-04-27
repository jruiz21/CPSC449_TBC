namespace TheBorrowedChapter.Dtos;

public class ReturnBookRequest
{
    public Guid BookId { get; set; }
    public Guid MemberId { get; set; }
}
