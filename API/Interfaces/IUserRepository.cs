using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
    Task<AppUser>                   GetUserByUsernameAsync(string username);
    Task<AppUser>                   GetUserByIdAsync(int id);
    Task<IEnumerable<AppUser>>      GetUsersAsync();
    Task<MemberDTO>                 GetMemberByUsernameAsync(string username);
    Task<MemberDTO>                 GetMemberByIdAsync(int id);
    Task<IEnumerable<MemberDTO>>    GetMembersAsync();
    Task<bool>                      EmailExistsAsync(string email);
    Task<bool>                      UsernameExistsAsync(string username);
    Task<bool>                      UpdateAsync(AppUser user);
}
