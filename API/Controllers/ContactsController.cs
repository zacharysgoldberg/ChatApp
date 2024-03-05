using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = "Admin,Member")]
public class ContactsController : BaseApiController
{
	private readonly IUserRepository _userRepository;
	private readonly IContactRepository _contactRepository;

	public ContactsController(IUserRepository userRepository, IContactRepository contactRepository)
	{
		_userRepository = userRepository;
		_contactRepository = contactRepository;
	}

	[HttpGet("{contactId}")] // /api/contacts/2
	public async Task<ActionResult<ContactDTO>> GetContact(int contactId)
	{
		int userId = User.GetUserId();
		ContactDTO contact = await _contactRepository.GetContactAsync(userId, contactId);

		return contact == null ? NotFound() : contact;
	}

	[HttpGet] // /api/contacts/1
	public async Task<ActionResult<IEnumerable<ContactDTO>>> GetContacts()
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

		return Ok(await _contactRepository.GetContactsAsync(user));
	}

	[HttpPost] // /api/contacts
	public async Task<ActionResult<MemberDTO>> AddContact([FromBody]
				ContactUsernameDTO contactUsernameDTO)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

		if (user == null ||
				contactUsernameDTO.UsernameOrEmail == user.UserName ||
				contactUsernameDTO.UsernameOrEmail == user.Email)
			return BadRequest("You cannot add yourself as a contact");

		MemberDTO contact = await _userRepository.GetMemberAsync(contactUsernameDTO.UsernameOrEmail);

		if (contact == null)
			return BadRequest($"{contactUsernameDTO.UsernameOrEmail} does not exist");

		if (!await _contactRepository.AddContactAsync(user, contact.Id))
			return BadRequest($"Failed to add contact {contactUsernameDTO.UsernameOrEmail}");

		return contact;
	}

	[HttpDelete("delete/{contactId}")] // /api/contacts/delete/2
	public async Task<IActionResult> DeleteContact(int contactId)
	{
		string usernameOrEmail = User.GetUsernameOrEmail();
		AppUser user = await _userRepository.GetUserAsync(usernameOrEmail);

		if (user == null)
			return NotFound();

		if (!await _contactRepository.DeleteContactAsync(user, contactId))
			return BadRequest("Failed to remove contact");

		return Ok();
	}
}