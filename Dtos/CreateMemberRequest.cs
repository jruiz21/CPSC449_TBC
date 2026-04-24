using System.ComponentModel.DataAnnotations;

namespace TheBorrowedChapter.Dtos;

public class CreateMemberRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}