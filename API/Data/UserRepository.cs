using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _userManager.Users
                        .Include(u => u.Contacts)
                        .SingleOrDefaultAsync(u => u.UserName == username);
    }

     public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _userManager.Users
                        .Include(u => u.Contacts)
                        .ToListAsync();
    }

    public async Task<MemberDTO> GetMemberAsync(string username)
    {
        return await _userManager.Users
                        .Where(user => user.UserName == username)
                        .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
                        .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<MemberDTO>> GetMembersAsync()
    {
        return await _userManager.Users
                        .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
                        .ToListAsync();
    }

    public async Task<IEnumerable<ContactDTO>> GetContactsAsync(string username)
    {   
        return _mapper.Map<IEnumerable<ContactDTO>>(await _userManager.Users
                        .Where(user => user.UserName == username)
                        .Select(c => c.Contacts)
                        .SingleOrDefaultAsync());
    }

    public async Task<bool> Update(AppUser user)
    {
        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;
    }
}
