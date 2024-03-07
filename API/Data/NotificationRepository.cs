using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class NotificationRepository : INotificationRepository
{
	private readonly DataContext _context;
	private readonly IMapper _mapper;

	public NotificationRepository(DataContext context, IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
	}

	public async Task CreateNotificationAsync(Notification notification)
	{
		await _context.Notifications.AddAsync(notification);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteNotificationAsync(int notificationId)
	{
		Notification notification = await _context.Notifications.FindAsync(notificationId);

		if (notification != null)
		{
			_context.Notifications.Remove(notification);
			await _context.SaveChangesAsync();
		}
	}

	public async Task<NotificationDTO> GetNotficationAsync(int notificationId)
	{
		return _mapper.Map<NotificationDTO>(await _context.Notifications.FindAsync(notificationId));
	}

	public async Task<IEnumerable<NotificationDTO>> GetUserNotifications(int userId)
	{
		IEnumerable<Notification> notifications = await _context.Notifications
				.Include(n => n.Sender)
				.Include(n => n.Recipient)
				.Include(n => n.GroupMessage)
				.Where(n => n.RecipientId == userId)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();

		var notificaitonDTOs = notifications.Select(n => new NotificationDTO
		{
			Id = n.Id,
			SenderId = n.SenderId,
			SenderUsername = n.Sender.UserName,
			RecipientId = n?.RecipientId,
			RecipientUsername = n?.Recipient.UserName,
			MessageId = n?.MessageId,
			GroupMessageId = n?.GroupMessageId,
			ChannelId = n.GroupMessage?.ChannelId,
			CreatedAt = n.CreatedAt,
			Content = n.Content
		});

		return notificaitonDTOs;
	}
}