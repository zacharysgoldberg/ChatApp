

using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public record ContactDTO
{
	public int Id { get; init; }
	public string UserName { get; init; }
	public string Email { get; init; }
	[DataType(DataType.PhoneNumber)]
	public string? PhonNumber { get; init; }
	public DateTime MemberSince { get; init; }
	public DateTime LastActive { get; init; }
	public string PhotoUrl { get; init; }
}


