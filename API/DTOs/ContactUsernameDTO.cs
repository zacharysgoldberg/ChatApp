
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public record AddContactDTO
(
		string? UsernameOrEmail,
		[DataType(DataType.PhoneNumber)]
		string? PhoneNumber
);
