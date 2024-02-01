using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
    Task<AppUser>                   GetUserByUsernameAsync(string username);
    Task<IEnumerable<AppUser>>      GetUsersAsync();
    Task<MemberDTO>                 GetMemberAsync(string username);
    Task<IEnumerable<MemberDTO>>    GetMembersAsync();
    Task<IEnumerable<ContactDTO>>   GetContactsAsync(string username);
    Task<bool>                      Update(AppUser user);
}
