using Microsoft.AspNetCore.Mvc;
using TheBorrowedChapter.Dtos;
using TheBorrowedChapter.Services;

namespace TheBorrowedChapter.Controllers;

[ApiController]
[Route("api/members")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _service;

    public MembersController(IMemberService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var members = await _service.GetAllAsync();
        return Ok(members);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(new ErrorResponse(result.ErrorMessage!));

        return Ok(result.Member);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest dto)
    {
        var result = await _service.CreateAsync(dto);
        if (!result.IsSuccess)
        {
            if (result.ErrorType == MemberErrorType.DuplicateEmail)
                return Conflict(new ErrorResponse(result.ErrorMessage!));

            return BadRequest(new ErrorResponse(result.ErrorMessage!));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Member!.Id }, result.Member);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        if (!result.IsSuccess)
        {
            if (result.ErrorType == MemberErrorType.NotFound)
                return NotFound(new ErrorResponse(result.ErrorMessage!));

            if (result.ErrorType == MemberErrorType.DuplicateEmail)
                return Conflict(new ErrorResponse(result.ErrorMessage!));

            return BadRequest(new ErrorResponse(result.ErrorMessage!));
        }

        return Ok(result.Member);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(new ErrorResponse(result.ErrorMessage!));

        return NoContent();
    }
}
