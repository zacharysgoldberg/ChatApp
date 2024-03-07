using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public record ForgotPasswordDTO
{
	[Required]
	[EmailAddress]
	public string Email { get; init; }
}