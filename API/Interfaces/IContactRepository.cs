using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Interfaces;

public interface IContactRepository
{
	Task<bool> AddContactAsync(AppUser user, int contactId);
	Task<ContactDTO> GetContactAsync(int userId, int contactId);
	Task<IEnumerable<ContactDTO>> GetContactsAsync(AppUser user);
	Task<bool> DeleteContactAsync(AppUser user, int contactId);
	Task<bool> UserContactExists(int userId, int contactId);
}
