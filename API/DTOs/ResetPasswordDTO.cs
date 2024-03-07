using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public record ResetPasswordDTO
{
	[Required]
	[DataType(DataType.Password)]
	public string Password { get; init; }

	[DataType(DataType.Password)]
	[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
	public string ConfirmPassword { get; init; }

	public string Email { get; init; }
	public string Token { get; init; }
}