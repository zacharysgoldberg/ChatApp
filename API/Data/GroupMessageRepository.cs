using API.DTOs;
using API.Interfaces;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using API.Helpers;

namespace API.Data
{
	public class GroupMessageRepository : IGroupMessageRepository
	{
		private readonly DataContext _context;
		private readonly IMapper _mapper;

		public GroupMessageRepository(DataContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<IEnumerable<GroupMessageDTO>> CreateGroupMessageChannelAsync(string channelName,
			int senderId, IEnumerable<int> contactIds)
		{
			// Check if the group message channel already exists
			IEnumerable<GroupMessage> existingChannel = await _context.GroupMessages
					.Include(gm => gm.Sender)
						.ThenInclude(gm => gm.Photo)
					.Include(gm => gm.Users)
					.Where(gm =>
							gm.Users.Count() == contactIds.Count()
							&& gm.Users.All(u => contactIds.Contains(u.Id)))
					.OrderBy(gm => gm.CreatedAt)
					.ToListAsync();

			if (existingChannel.Any())
			{
				IEnumerable<GroupMessageDTO> groupMessageDTOs =
					_mapper.Map<IEnumerable<GroupMessage>, IEnumerable<GroupMessageDTO>>(existingChannel);

				return groupMessageDTOs;
			}

			AppUser sender = await _context.Users.FindAsync(senderId);

			// If the channel/thread doesn't exist, create it and associate contacts
			var groupMessageChannel = new GroupMessage
			{
				ChannelId = Guid.NewGuid(),
				ChannelName = channelName,
				SenderId = senderId,
				Content = $"Welcom to {channelName}" // Set initial group message
			};

			groupMessageChannel.Users.Add(sender); // Add sender to the channel

			// Retrieve and associate contacts with the channel
			foreach (int contactId in contactIds)
			{
				AppUser contact = await _context.Users.FindAsync(contactId);
				groupMessageChannel.Users.Add(contact);
			}
			// Add the channel to the context and save changes
			await _context.GroupMessages.AddAsync(groupMessageChannel);
			await _context.SaveChangesAsync();
			GroupMessageDTO newChannel = _mapper.Map<GroupMessageDTO>(groupMessageChannel);

			return new List<GroupMessageDTO> { newChannel };
		}

		public async Task<bool> CreateGroupMessageAsync(GroupMessage groupMessage)
		{
			_context.GroupMessages.Add(groupMessage);
			return await SaveAllAsync();
		}

		public async Task<IEnumerable<GroupMessageDTO>> GetGroupMessageChannelAsync(Guid channelId)
		{
			IEnumerable<GroupMessage> groupMessages = await _context.GroupMessages
					.Include(gm => gm.Sender)
						.ThenInclude(gm => gm.Photo)
					.Include(gm => gm.Users)
					.Where(gm => gm.ChannelId == channelId)
					.OrderBy(gm => gm.CreatedAt)
					.ToListAsync();

			IEnumerable<GroupMessageDTO> groupMessageDTOs =
					_mapper.Map<IEnumerable<GroupMessage>, IEnumerable<GroupMessageDTO>>(groupMessages);

			return groupMessageDTOs;
		}

		public async Task<IEnumerable<GroupMessageDTO>> GetGroupMessageChannelNamesAsync
			(int userId)
		{
			// Retrieve group message channels where the user is a member
			IEnumerable<GroupMessageDTO> groupMessageChannels = await _context.GroupMessages
					.Include(gm => gm.Users)
						.ThenInclude(gm => gm.Photo)
					.Where(gm => gm.Users.Any(u => u.Id == userId))
					.OrderByDescending(gm => gm.CreatedAt)
					.Select(gm => new GroupMessageDTO
					{
						ChannelId = gm.ChannelId,
						ChannelName = gm.ChannelName,
						Contacts = gm.Users.Select(u => _mapper.Map<ContactDTO>(u)).ToList()
					})
					.ToListAsync();

			return groupMessageChannels;
		}

		public async Task<IEnumerable<ContactDTO>> GetGroupMessageChannelContactsAsync(Guid channelId)
		{
			IEnumerable<ContactDTO> contacts = await _context.GroupMessages
				.Include(gm => gm.Users)
					.ThenInclude(gm => gm.Photo)
				.Where(gm => gm.ChannelId == channelId)
				.SelectMany(gm => gm.Users)
				.Select(u => _mapper.Map<ContactDTO>(u))
				.ToListAsync();

			return contacts;
		}

		public async Task<bool> DeleteGroupMessageAsync(GroupMessage groupMessage)
		{
			_context.GroupMessages.Remove(groupMessage);
			return await SaveAllAsync();
		}

		public async Task<bool> SaveAllAsync()
		{
			return await _context.SaveChangesAsync() > 0;
		}
	}
}
