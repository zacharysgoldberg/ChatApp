﻿using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
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

	public async Task<IEnumerable<MessageDTO>> CreateMessageThreadAsync(int senderId, int recipientId)
	{
		// Check if a message thread already exists between sender and recipient
		IEnumerable<Message> existingThread = await _context.Messages
				.Include(m => m.Sender)
					.ThenInclude(m => m.Photo)
				.Where(m =>
					(m.RecipientId == senderId && m.SenderId == recipientId) ||
					(m.RecipientId == recipientId && m.SenderId == senderId))
				.OrderBy(m => m.CreatedAt)
				.ToListAsync();

		// If an existing thread is found, fetch the messages associated
		if (existingThread.Any())
		{
			var usersInfo = await _context.Users
					.Where(u => u.Id == senderId || u.Id == recipientId)
					.Select(u => new
					{
						Id = u.Id,
						UserName = u.UserName,
						PhotoUrl = u.Photo.Url
					})
					.ToListAsync();

			IEnumerable<MessageDTO> messageDTOs = existingThread.Select(message => new MessageDTO
			{
				// Map message properties to DTO properties
				Id = message.Id,
				SenderId = message.SenderId,
				SenderUsername = usersInfo.FirstOrDefault(u => u.Id == message.SenderId).UserName,
				SenderPhotoUrl = usersInfo.FirstOrDefault(u => u.Id == message.SenderId)?.PhotoUrl,
				RecipientId = message.RecipientId,
				RecipientUsername = usersInfo.FirstOrDefault(u => u.Id == message.RecipientId).UserName,
				RecipientPhotoUrl = usersInfo.FirstOrDefault(u => u.Id == message.RecipientId)?.PhotoUrl,
				Content = message.Content,
				CreatedAt = message.CreatedAt,
			});

			return messageDTOs;
		}
		// No existing thread found, return an empty collection
		return Enumerable.Empty<MessageDTO>();
	}

	public async Task<bool> CreateMessageAsync(Message message)
	{
		_context.Messages.Add(message);
		return await SaveAllAsync();
	}

	public async Task<IEnumerable<MessageDTO>> GetMessageThreadAsync(int userId, int recipientId)
	{
		var usersInfo = await _context.Users
			.Where(u => u.Id == userId || u.Id == recipientId)
			.Select(u => new
			{
				Id = u.Id,
				UserName = u.UserName,
				PhotoUrl = u.Photo.Url
			})
			.ToListAsync();

		var messages = await _context.Messages
			.Include(m => m.Sender)
				.ThenInclude(u => u.Photo)
			.Include(m => m.Recipient)
				.ThenInclude(u => u.Photo)
			.Where(m =>
				(m.RecipientId == userId && m.SenderId == recipientId) ||
				(m.RecipientId == recipientId && m.SenderId == userId))
			.OrderBy(m => m.CreatedAt)
			.ToListAsync();

		var messageDTOs = messages.Select(message => new MessageDTO
		{
			Id = message.Id,
			SenderId = message.SenderId,
			SenderUsername = usersInfo.FirstOrDefault(u => u.Id == message.SenderId).UserName,
			SenderPhotoUrl = usersInfo.FirstOrDefault(u => u.Id == message.SenderId)?.PhotoUrl,
			RecipientId = message.RecipientId,
			RecipientUsername = usersInfo.FirstOrDefault(u => u.Id == message.RecipientId).UserName,
			RecipientPhotoUrl = usersInfo.FirstOrDefault(u => u.Id == message.RecipientId)?.PhotoUrl,
			Content = message.Content,
			CreatedAt = message.CreatedAt
		}).ToList();

		return messageDTOs;
	}

	public async Task<IEnumerable<ContactDTO>> GetContactsWithMessageThreadAsync(int userId)
	{
		IEnumerable<int> usersIds = await _context.Messages
				.Where(m => m.RecipientId == userId || m.SenderId == userId)
				.Select(m => m.RecipientId == userId ? m.SenderId : m.RecipientId)
				.Distinct()
				.ToListAsync();

		if (usersIds == null)
			return null;

		List<ContactDTO> contactDTOs = new();

		foreach (int id in usersIds)
		{
			MemberDTO memberDTO = await _userRepository.GetMemberByIdAsync(id);
			contactDTOs.Add(_mapper.Map<ContactDTO>(memberDTO));
		}
		return contactDTOs;
	}

	public async Task<bool> DeleteMessageAsync(Message message)
	{
		_context.Remove(message);
		return await SaveAllAsync();
	}

	public async Task<bool> SaveAllAsync()
	{
		return await _context.SaveChangesAsync() > 0;
	}
}
