using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Interfaces;

public interface IUserRepository
{
	Task<AppUser> GetUserAsync(string usernameOrEmail);
	Task<AppUser> GetUserByIdAsync(int id);
	Task<MemberDTO> GetMemberAsync(string usernameOrEmail);
	Task<MemberDTO> GetMemberByPhoneNumberAsync(string phoneNumber);
	Task<MemberDTO> GetMemberByIdAsync(int id);
	Task<IEnumerable<MemberDTO>> GetMembersAsync();
	Task<bool> UserExists(int id);
	Task<bool> EmailExistsAsync(string email);
	Task<bool> UsernameExistsAsync(string username);
	Task<bool> PhoneNumberExistsAsync(string phoneNumber);
	Task<bool> PasswordMatchesAsync(AppUser user, string password);
	Task<IdentityResult> CreateUserAsync(AppUser user, string password);
	Task<IdentityResult> AddRoleToUserAsync(AppUser user, string role);
	Task<IdentityResult> UpdateUserAsync(AppUser user);
	Task<IdentityResult> UpdatePasswordAsync(AppUser user, string currentPassword,
		string newPassword);
}
