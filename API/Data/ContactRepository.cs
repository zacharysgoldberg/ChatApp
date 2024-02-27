using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Services;

public class ContactRepository : IContactRepository
{
	private readonly IUserRepository _userRepository;
	private readonly DataContext _context;
	private readonly IMapper _mapper;

	public ContactRepository(IUserRepository userRepository, DataContext context,
			IMapper mapper)
	{
		_userRepository = userRepository;
		_context = context;
		_mapper = mapper;
	}

	public async Task<bool> AddContactAsync(AppUser user, int contactId)
	{
		if (user.Contacts.Any(c => c.Id == contactId))
			// Contact already exists, return false indicating failure
			return false;

		Contact contact = await _context.Contacts.FindAsync(contactId);

		// Create a new Contact entity if it doesn't exist
		if (contact == null)
		{
			contact = new Contact { Id = contactId, UserId = user.Id };
			await _context.Contacts.AddAsync(contact);
			await _context.SaveChangesAsync();
		}

		user.Contacts.Add(contact);
		IdentityResult addedContactResult = await _userRepository.UpdateUserAsync(user);

		return addedContactResult.Succeeded;
	}

	public async Task<ContactDTO> GetContactAsync(int userId, int contactId)
	{
		MemberDTO memberDTO = await _userRepository.GetMemberByIdAsync(contactId);

		if (memberDTO == null)
			return null;

		ContactDTO contactDTO = _mapper.Map<ContactDTO>(memberDTO);

		return contactDTO;
	}

	public async Task<IEnumerable<ContactDTO>> GetContactsAsync(int userId)
	{
		AppUser user = await _userRepository.GetUserByIdAsync(userId);
		ICollection<Contact> contacts = user.Contacts;

		// DebugUtil.PrintDebug(ref contacts);

		if (contacts == null)
			return Enumerable.Empty<ContactDTO>();

		List<ContactDTO> contactDTOs = new();

		// Iterate through the contacts and fetch corresponding Member DTOs
		foreach (Contact contact in contacts)
		{
			MemberDTO memberDTO = await _userRepository.GetMemberByIdAsync(contact.Id);
			contactDTOs.Add(_mapper.Map<ContactDTO>(memberDTO));
		}

		return contactDTOs;
	}

	public async Task<bool> DeleteContactAsync(AppUser user, int contactId)
	{
		Contact contact = user.Contacts.FirstOrDefault(c => c.Id == contactId);

		if (contact == null)
			return false;

		// _context.Contacts.Remove(contact);
		// await _context.SaveChangesAsync();
		user.Contacts.Remove(contact);
		IdentityResult updateUserResult = await _userRepository.UpdateUserAsync(user);

		return updateUserResult.Succeeded;
	}

	public async Task<bool> UserContactExists(int userId, int contactId)
	{
		// return await _context.UserContacts
		// 		.AnyAsync(uc => uc.AppUserId == userId && uc.ContactId == contactId);

		AppUser user = await _userRepository.GetUserByIdAsync(userId);
		Contact contact = user.Contacts.FirstOrDefault(c => c.Id == contactId);

		return contact != null;
	}
}