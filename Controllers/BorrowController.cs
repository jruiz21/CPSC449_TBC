using Microsoft.AspNetCore.Mvc;
using TheBorrowedChapter.Dtos;
using TheBorrowedChapter.Services;

namespace TheBorrowedChapter.Controllers;

[ApiController]
[Route("api/borrows")]
public class BorrowController : ControllerBase
{
    private readonly IBorrowService _borrowService;

    public BorrowController(IBorrowService borrowService)
    {
        _borrowService = borrowService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetAll()
    {
        var records = await _borrowService.GetAllAsync();
        return Ok(records);
    }

    [HttpGet("member/{memberId:guid}")]
    public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetMemberHistory(Guid memberId)
    {
        if (memberId == Guid.Empty)
            return BadRequest(new ErrorResponse("MemberId is required."));

        var records = await _borrowService.GetMemberHistoryAsync(memberId);
        return Ok(records);
    }

    [HttpPost]
    public async Task<ActionResult<BorrowRecordResponse>> BorrowBook([FromBody] BorrowBookRequest request)
    {
        var result = await _borrowService.BorrowBookAsync(request);

        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                BorrowErrorType.NotFound => NotFound(new ErrorResponse(result.ErrorMessage!)),
                BorrowErrorType.AlreadyBorrowed => Conflict(new ErrorResponse(result.ErrorMessage!)),
                BorrowErrorType.NoAvailableCopies => Conflict(new ErrorResponse(result.ErrorMessage!)),
                _ => BadRequest(new ErrorResponse(result.ErrorMessage!))
            };
        }

        return StatusCode(StatusCodes.Status201Created, result.Record);
    }

    [HttpPost("return")]
    public async Task<ActionResult<BorrowRecordResponse>> ReturnBook([FromBody] ReturnBookRequest request)
    {
        var result = await _borrowService.ReturnBookAsync(request);

        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                BorrowErrorType.NotFound => NotFound(new ErrorResponse(result.ErrorMessage!)),
                BorrowErrorType.NotBorrowed => Conflict(new ErrorResponse(result.ErrorMessage!)),
                _ => BadRequest(new ErrorResponse(result.ErrorMessage!))
            };
        }

        return Ok(result.Record);
    }
}
