using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface INotificationRepository
{
	Task CreateNotificationAsync(Notification notification);
	Task DeleteNotificationAsync(int notificationId);
	Task<NotificationDTO> GetNotficationAsync(int notificationId);
	Task<IEnumerable<NotificationDTO>> GetUserNotifications(AppUser user);
}