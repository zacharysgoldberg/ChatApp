using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IContactService
{
    Task<bool> AddContactAsync(AppUser user, int contactId);
    Task<ContactDTO> GetContactAsync(int userId, int contactId);
    Task<IEnumerable<ContactDTO>> GetContactsAsync(int userId);
    Task<bool> DeleteContactAsync(AppUser user, int contactId);
    Task<bool> UserContactExists(int userId, int contactId);
}
