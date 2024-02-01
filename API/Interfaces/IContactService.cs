using API.Entities;

namespace API.Interfaces;

public interface IContactService
{
    Task<bool> AddContactAsync(AppUser user, string contactUsername);
    Task<bool> DeleteContactAsync(AppUser user, string contactUsername);
}
