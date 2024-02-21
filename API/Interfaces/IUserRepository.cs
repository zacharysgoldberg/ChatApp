using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
	Task<AppUser> GetUserAsync(string usernameOrEmail);
	Task<AppUser> GetUserByIdAsync(int id);
	Task<IEnumerable<AppUser>> GetUsersAsync();
	Task<MemberDTO> GetMemberAsync(string usernameOrEmail);
	Task<MemberDTO> GetMemberByIdAsync(int id);
	Task<IEnumerable<MemberDTO>> GetMembersAsync();
	Task<bool> EmailExistsAsync(string email);
	Task<bool> UsernameExistsAsync(string username);
	Task<bool> CreateUserAsync(AppUser user, string password);
	Task<bool> UpdateUserAsync(AppUser user);
	Task<bool> AddRoleToUserAsync(AppUser user, string role);
	Task<bool> PasswordMatchesAsync(AppUser user, string password);
}
