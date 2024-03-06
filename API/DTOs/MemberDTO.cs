
using System.ComponentModel.DataAnnotations;
using API.Entities;

namespace API.DTOs;

public record MemberDTO
(
		int Id,

		string UserName,
		string Email,
		[DataType(DataType.PhoneNumber)]
		string? PhoneNumber,
		string RefreshToken,
		DateTime RefreshTokenExpiryTime,
		DateTime MemberSince,
		DateTime LastActive,
		string PhotoUrl,
		PhotoDTO Photo,
		ICollection<Contact> Contacts
);