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
                        .SingleOrDefaultAsync(u => u.UserName == usernameOrEmail || 
                                            u.Email == usernameOrEmail.ToLower());
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {        
        return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
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
                        .SingleOrDefaultAsync();
    }

    
    public async Task<MemberDTO> GetMemberByIdAsync(int id)
    {
        return await _userManager.Users
                        .Where(user => user.Id == id)
                        .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
                        .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<MemberDTO>> GetMembersAsync()
    {
        return await _userManager.Users
                        .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
                        .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
            return true;
        return false;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
         var user = await _userManager.FindByNameAsync(username);

        if (user != null)
            return true;
        return false;
    }

    public async Task<bool> UpdateAsync(AppUser user)
    {
        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;
    }
}
