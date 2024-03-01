using API.DTOs;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
	public class AppUser : IdentityUser<int>
	{
		public string RefreshToken { get; set; }
		public DateTime RefreshTokenExpiryTime { get; set; }
		public DateTime Created { get; set; } = DateTime.UtcNow; // replace with MemeberSince
		public DateTime LastActive { get; set; } = DateTime.UtcNow;
		public Photo Photo { get; set; }
		// public ICollection<UserContact> UserContacts { get; set; }
		public ICollection<Contact> Contacts { get; set; }
		public ICollection<Message> MessagesSent { get; set; }
		public ICollection<Message> MessagesReceived { get; set; }
		public ICollection<GroupMessage> GroupMessages { get; set; }
		// public ICollection<Notification> NotificationsSent { get; set; } = new List<Notification>();
		// public ICollection<Notification> NotificationsReceived { get; set; } = new List<Notification>();
	}
}

