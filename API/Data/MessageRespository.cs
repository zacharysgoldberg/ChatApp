using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API;

public class MessageRespository : IMessageRepository
{
	private readonly DataContext _context;
	private readonly IUserRepository _userRepository;
	private readonly IMapper _mapper;

	public MessageRespository(DataContext context, IUserRepository userRepository, IMapper mapper)
	{
		_context = context;
		_userRepository = userRepository;
		_mapper = mapper;
	}

	public async Task<IEnumerable<MessageDTO>> AddMessageThreadAsync(int senderId, int recipientId)
	{
		// Check if a message thread already exists between sender and recipient
		IEnumerable<Message> existingThread = await _context.Messages
				.Include(m => m.Sender)
				.ThenInclude(m => m.Photo)
				.Where(m =>
					(m.RecipientId == senderId && m.SenderId == recipientId) ||
					(m.RecipientId == recipientId && m.SenderId == senderId))
				.OrderBy(m => m.MessageSent)
				.ToListAsync();

		// If an existing thread is found, fetch the messages associated with that thread
		if (existingThread.Any())
		{
			var messageDTOs = existingThread.Select(message => new MessageDTO
			{
				// Map message properties to DTO properties
				Id = message.Id,
				SenderId = message.SenderId,
				RecipientId = message.RecipientId,
				Content = message.Content,
				MessageSent = message.MessageSent,
				// Project sender and recipient usernames using foreign keys (may need to update to LinQ)
				SenderUsername = message.Sender.UserName,
				RecipientUsername = message.Recipient.UserName
			});

			return messageDTOs;
		}
		else
			// No existing thread found, return an empty collection
			return Enumerable.Empty<MessageDTO>();
	}

	public void AddMessageAsync(Message message)
	{
		_context.Messages.Add(message);
	}

	public void DeleteMessageAsync(Message message)
	{
		_context.Remove(message);
	}

	public async Task<Message> GetMessageAsync(int id)
	{
		return await _context.Messages.FindAsync(id);
	}

	public async Task<PagedList<MessageDTO>> GetMessagesAsync(MessageParams messageParams)
	{
		IQueryable<Message> query = _context.Messages
				.OrderByDescending(x => x.MessageSent)
				.AsQueryable();

		query = messageParams.Container switch
		{
			"Inbox" => query.Where(u => u.RecipientId == messageParams.Id),
			"Outbox" => query.Where(u => u.SenderId == messageParams.Id),
			_ => query.Where(u => u.RecipientId == messageParams.Id)
		};

		IQueryable<MessageDTO> messages = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

		return await PagedList<MessageDTO>
				.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
	}

	public async Task<IEnumerable<MessageDTO>> GetMessageThreadAsync(int currentUserId, int recipientId)
	{
		// Query sender and recipient usernames and photoUrls
		string senderUsername = _context.Users
						.Where(u => u.Id == currentUserId)
						.Select(u => u.UserName)
						.FirstOrDefault();
		string recipientUsername = _context.Users
						.Where(u => u.Id == recipientId)
						.Select(u => u.UserName)
						.FirstOrDefault();
		string senderPhotoUrl = _context.Users
						.Where(u => u.Id == currentUserId)
						.Select(u => u.Photo.Url)
						.FirstOrDefault();
		string recipientPhotoUrl = _context.Users
						.Where(u => u.Id == recipientId)
						.Select(u => u.Photo.Url)
						.FirstOrDefault();

		IEnumerable<MessageDTO> messages = await _context.Messages
				.Include(m => m.Sender)
				.ThenInclude(m => m.Photo)
				.Where(m =>
					(m.RecipientId == currentUserId && m.SenderId == recipientId) ||
					(m.RecipientId == recipientId && m.SenderId == currentUserId))
				.OrderBy(m => m.MessageSent)
				.Select(message => new MessageDTO
				{
					// Map message properties to DTO properties
					Id = message.Id,
					SenderId = message.SenderId,
					SenderUsername = senderUsername,
					SenderPhotoUrl = senderPhotoUrl,
					RecipientId = message.RecipientId,
					RecipientUsername = recipientUsername,
					RecipientPhotoUrl = recipientPhotoUrl,
					Content = message.Content,
					MessageSent = message.MessageSent
				})
				.ToListAsync();

		return messages;
	}

	public async Task<IEnumerable<ContactDTO>> GetContactsWithMessageThreadAsync(int currentUserId)
	{
		IEnumerable<int> usersIds = await _context.Messages
				.Where(m => m.RecipientId == currentUserId || m.SenderId == currentUserId)
				.Select(m => m.RecipientId == currentUserId ? m.SenderId : m.RecipientId)
				.Distinct()
				.ToListAsync();

		if (usersIds == null)
			return null;

		List<ContactDTO> contactDTOs = new();

		foreach (int userId in usersIds)
		{
			MemberDTO memberDTO = await _userRepository.GetMemberByIdAsync(userId);
			contactDTOs.Add(_mapper.Map<ContactDTO>(memberDTO));
		}
		return contactDTOs;
	}

	public async Task<bool> SaveAllAsync()
	{
		return await _context.SaveChangesAsync() > 0;
	}
}
