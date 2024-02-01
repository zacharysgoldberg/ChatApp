using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Services;

public class ContactService : IContactService
{
    private readonly IUserRepository _userRepository;
    public ContactService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> AddContactAsync(AppUser user, string contactUsername)
    {
         MemberDTO memberDTO = await _userRepository.GetMemberAsync(contactUsername);

         if(memberDTO == null)
            return false;

         var contact = new Contact
        {
            UserName = memberDTO.UserName,
            Email = memberDTO.Email,
            Photo = memberDTO.Photo,
            LastActive = memberDTO.LastActive
        };

        user.Contacts.Add(contact);

        return await _userRepository.Update(user);
    }

    public async Task<bool> DeleteContactAsync(AppUser user, string contactUsername)
    {
        Contact contact = user.Contacts.Find(c => c.UserName == contactUsername);

        if(contact == null)
            return false;

        user.Contacts.Remove(contact);

        return await _userRepository.Update(user);
    }
}