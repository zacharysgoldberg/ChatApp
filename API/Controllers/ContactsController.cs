using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ContactsController: BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IContactRepository _contactRepository;

    public ContactsController(IUserRepository userRepository, IContactRepository contactRepository)
    {
        _userRepository     = userRepository;
        _contactRepository  = contactRepository;
    }

        
    [HttpGet("{contactId}")] // /api/contacts/2
    public async Task<ActionResult<ContactDTO>> GetContact(int contactId)
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();
        MemberDTO user          = await _userRepository.GetMemberAsync(usernameOrEmail);
        ContactDTO contact      = await _contactRepository.GetContactAsync(user.Id, contactId);

        if(contact == null)
            return NotFound();

        return contact;
    }

    [HttpGet] // /api/contacts/1
    public async Task<ActionResult<IEnumerable<ContactDTO>>> GetContacts()
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();
        MemberDTO user          = await _userRepository.GetMemberAsync(usernameOrEmail);

        return Ok(await _contactRepository.GetContactsAsync(user.Id));
    }

    [HttpPost] // /api/contacts
    public async Task<ActionResult<MemberDTO>> AddContact([FromBody] 
        ContactUsernameDTO contactUsernameDTO)
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();
        AppUser user            = await _userRepository.GetUserAsync(usernameOrEmail);

        if(user == null || 
            contactUsernameDTO.UsernameOrEmail == user.UserName || 
            contactUsernameDTO.UsernameOrEmail == user.Email)
            return BadRequest("You cannot add yourself as a contact");

        MemberDTO contact = await _userRepository.GetMemberAsync(contactUsernameDTO.UsernameOrEmail);

        if(contact == null)
            return BadRequest($"{contactUsernameDTO.UsernameOrEmail} does not exist");

        bool userContactExists = await _contactRepository.UserContactExists(user.Id, contact.Id);

        if(contact == null || userContactExists)
            return BadRequest($"{contactUsernameDTO.UsernameOrEmail} already exists in contact list");

        bool succeeded = await _contactRepository.AddContactAsync(user, contact.Id);

        if(!succeeded)
            return BadRequest($"\nFailed to add contact {contactUsernameDTO.UsernameOrEmail}");
            
        return contact;
    }

    [HttpPost("delete/{contactId}")] // /api/contacts/delete/2
    public async Task<IActionResult> DeleteContact(int contactId)
    {
        string usernameOrEmail  = User.GetUsernameOrEmail();
        AppUser user            = await _userRepository.GetUserAsync(usernameOrEmail);

        if(user == null)
            return NotFound();

        bool succeeded = await _contactRepository.DeleteContactAsync(user, contactId);

        if(!succeeded)
            return BadRequest("\nFailed to remove contact");
            
        return Ok();
    }
}