using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUserRepository
{
	private readonly UserManager<AppUser> _userManager;
	private readonly IMapper _mapper;

	public UserRepository(UserManager<AppUser> userManager, IMapper mapper)
	{
		_userManager = userManager;
		_mapper = mapper;
	}

	public async Task<AppUser> GetUserAsync(string usernameOrEmail)
	{
		return await _userManager.Users
										.Include(u => u.Contacts) // include contacts list
										.Include(u => u.Photo)
										.FirstOrDefaultAsync(u => u.UserName == usernameOrEmail ||
																				u.Email == usernameOrEmail.ToLower());
	}

	public async Task<AppUser> GetUserByIdAsync(int id)
	{
		return await _userManager.Users
										.Include(u => u.Photo)
										.FirstOrDefaultAsync(u => u.Id == id);
	}

	public async Task<MemberDTO> GetMemberAsync(string usernameOrEmail)
	{
		return await _userManager.Users
										.Where(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail)
										.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
										.FirstOrDefaultAsync();
	}

	public async Task<MemberDTO> GetMemberByPhoneNumberAsync(string phoneNumber)
	{
		return await _userManager.Users
										.Where(u => u.PhoneNumber == phoneNumber)
										.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
										.FirstOrDefaultAsync();
	}

	public async Task<MemberDTO> GetMemberByIdAsync(int id)
	{
		return await _userManager.Users
										.Where(user => user.Id == id)
										.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
										.FirstOrDefaultAsync();
	}

	public async Task<IEnumerable<MemberDTO>> GetMembersAsync()
	{
		return await _userManager.Users
										.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
										.ToListAsync();
	}

	public async Task<bool> UserExists(int id)
	{
		return await _userManager.Users.FirstOrDefaultAsync(user => user.Id == id) != null;
	}

	public async Task<bool> EmailExistsAsync(string email)
	{
		return await _userManager.FindByEmailAsync(email) != null;
	}

	public async Task<bool> UsernameExistsAsync(string username)
	{
		return await _userManager.FindByNameAsync(username) != null;
	}

	public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
	{
		return await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber) != null;
	}

	public async Task<bool> PasswordMatchesAsync(AppUser user, string password)
	{
		return await _userManager.CheckPasswordAsync(user, password);
	}

	public async Task<IdentityResult> CreateUserAsync(AppUser user, string password)
	{
		return await _userManager.CreateAsync(user, password);
	}

	public async Task<IdentityResult> AddRoleToUserAsync(AppUser user, string role)
	{
		return await _userManager.AddToRoleAsync(user, role);
	}

	public async Task<IdentityResult> UpdateUserAsync(AppUser user)
	{
		return await _userManager.UpdateAsync(user);
	}

	public async Task<IdentityResult> UpdatePasswordAsync(AppUser user, string currentPassword,
		string newPassword)
	{
		return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
	}
}
