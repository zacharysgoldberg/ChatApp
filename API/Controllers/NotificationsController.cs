using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
	[Authorize(Roles = "Admin,Member")]
	public class NotificationsController : BaseApiController
	{
		private readonly INotificationRepository _notificationRepository;
		private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;

		public NotificationsController(INotificationRepository notificationRepository,
			IUserRepository userRepository, IMapper mapper)
		{
			_notificationRepository = notificationRepository;
			_userRepository = userRepository;
			_mapper = mapper;
		}

		[HttpGet("{notificationId}")]
		public async Task<ActionResult<NotificationDTO>> GetNotification(int notificationId)
		{
			var notification = await _notificationRepository.GetNotficationAsync(notificationId);
			if (notification == null)
			{
				return NotFound();
			}

			var notificationDto = _mapper.Map<NotificationDTO>(notification);
			return Ok(notificationDto);
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetUserNotifications()
		{
			int userId = int.Parse(User.FindFirst("id").Value);
			AppUser user = await _userRepository.GetUserByIdAsync(userId);

			IEnumerable<NotificationDTO> notifications = await _notificationRepository.GetUserNotifications(user);

			return Ok(notifications);
		}

		[HttpDelete("{notificationId}")]
		public async Task<IActionResult> DeleteNotification(int notificationId)
		{
			var notification = await _notificationRepository.GetNotficationAsync(notificationId);
			if (notification == null)
			{
				return NotFound();
			}

			await _notificationRepository.DeleteNotificationAsync(notificationId);
			return NoContent();
		}
	}
}
