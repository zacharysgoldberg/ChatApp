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
										// .Include(u => u.UserContacts) // include contacts list
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

	public async Task<IEnumerable<AppUser>> GetUsersAsync()
	{
		return await _userManager.Users
										.Include(u => u.UserContacts)
										.ToListAsync();
	}

	public async Task<MemberDTO> GetMemberAsync(string usernameOrEmail)
	{
		return await _userManager.Users
										.Where(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail)
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

	public async Task<bool> EmailExistsAsync(string email)
	{
		AppUser user = await _userManager.FindByEmailAsync(email);

		if (user != null)
			return true;
		return false;
	}

	public async Task<bool> UsernameExistsAsync(string username)
	{
		AppUser user = await _userManager.FindByNameAsync(username);

		if (user != null)
			return true;
		return false;
	}

	public async Task<bool> CreateUserAsync(AppUser user, string password)
	{
		IdentityResult result = await _userManager.CreateAsync(user, password);

		return result.Succeeded;
	}

	public async Task<bool> UpdateUserAsync(AppUser user)
	{
		var result = await _userManager.UpdateAsync(user);

		return result.Succeeded;
	}

	public async Task<bool> AddRoleToUserAsync(AppUser user, string role)
	{
		var result = await _userManager.AddToRoleAsync(user, role);

		return result.Succeeded;
	}

	public async Task<bool> PasswordMatchesAsync(AppUser user, string password)
	{
		return await _userManager.CheckPasswordAsync(user, password);
	}
}
