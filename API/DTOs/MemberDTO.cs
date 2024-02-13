
using API.Entities;

namespace API.DTOs;

public record MemberDTO(
    int Id,
    string UserName,
    string Email,
    string? PhoneNumber,
    string RefreshToken,
    DateTime RefreshTokenExpiryTime,
    DateTime Created,
    DateTime LastActive,
    string PhotoUrl,
    PhotoDTO Photo,
    ICollection<UserContact> UserContacts
);