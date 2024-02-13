
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public record RegisterDTO
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public required string Email {get; init;}

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public required string Password {get; init;}

    [Required(ErrorMessage = "Password Confirmation is required.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; init; }
}
