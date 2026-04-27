using Microsoft.AspNetCore.Mvc;
using TheBorrowedChapter.Dtos;
using TheBorrowedChapter.Services;

namespace TheBorrowedChapter.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookResponse>>> GetBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookResponse>> GetBookById(Guid id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book is null)
            return NotFound(new ErrorResponse($"Book with id {id} was not found."));

        return Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> CreateBook([FromBody] CreateBookRequest request)
    {
        var result = await _bookService.CreateBookAsync(request);

        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                BookErrorType.DuplicateISBN => Conflict(new ErrorResponse(result.ErrorMessage!)),
                _ => BadRequest(new ErrorResponse(result.ErrorMessage!))
            };
        }

        return CreatedAtAction(nameof(GetBookById), new { id = result.Book!.Id }, result.Book);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookResponse>> UpdateBook(Guid id, [FromBody] UpdateBookRequest request)
    {
        var result = await _bookService.UpdateBookAsync(id, request);

        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                BookErrorType.NotFound => NotFound(new ErrorResponse(result.ErrorMessage!)),
                BookErrorType.DuplicateISBN => Conflict(new ErrorResponse(result.ErrorMessage!)),
                _ => BadRequest(new ErrorResponse(result.ErrorMessage!))
            };
        }

        return Ok(result.Book);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        var result = await _bookService.DeleteBookAsync(id);
        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                BookErrorType.NotFound => NotFound(new ErrorResponse(result.ErrorMessage!)),
                _ => BadRequest(new ErrorResponse(result.ErrorMessage!))
            };
        }

        return NoContent();
    }
}
