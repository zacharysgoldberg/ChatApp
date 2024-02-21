using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
		Contact contact = await _context.Contacts.FindAsync(contactId);

		// Create a new Contact entity if it doesn't exist
		if (contact == null)
		{
			var newContact = new Contact { Id = contactId };
			var addedContact = await _context.Contacts.AddAsync(newContact);

			if (addedContact == null)
				return false;
		}

		var userContact = new UserContact
		{
			AppUserId = user.Id,
			ContactId = contactId
		};

		_context.UserContacts.Add(userContact);
		await _context.SaveChangesAsync();

		return await _userRepository.UpdateUserAsync(user);
	}

	public async Task<ContactDTO> GetContactAsync(int userId, int contactId)
	{
		Contact contact = await _context.UserContacts
				.Where(uc => uc.AppUserId == userId && uc.ContactId == contactId)
				.Select(uc => uc.Contact)
				.FirstOrDefaultAsync();

		if (contact == null)
			return null;

		// Once you have the UserContact/Contact entry, you can access the corresponding Member DTO
		MemberDTO memberDTO = await _userRepository.GetMemberByIdAsync(contact.Id);
		// Then cast the member into a Contact DTO
		ContactDTO contactDTO = _mapper.Map<ContactDTO>(memberDTO);

		return contactDTO;
	}

	public async Task<IEnumerable<ContactDTO>> GetContactsAsync(int userId)
	{
		IEnumerable<Contact> contacts = await _context.UserContacts
				.Where(uc => uc.AppUserId == userId)
				.Select(uc => uc.Contact)
				.ToListAsync();

		if (contacts == null)
			return null;

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
		UserContact userContact = await _context.UserContacts
				.Where(uc => uc.AppUserId == user.Id && uc.ContactId == contactId)
				.FirstOrDefaultAsync();

		if (userContact == null)
			return false;

		_context.UserContacts.Remove(userContact);

		return await _userRepository.UpdateUserAsync(user);
	}

	public async Task<bool> UserContactExists(int userId, int contactId)
	{
		return await _context.UserContacts
				.AnyAsync(uc => uc.AppUserId == userId && uc.ContactId == contactId);
	}
}