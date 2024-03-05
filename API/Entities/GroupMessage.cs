using System.Numerics;

namespace API.Entities;

public class GroupMessage
{
	public int Id { get; set; }
	public Guid ChannelId { get; set; }
	public string ChannelName { get; set; }
	public int SenderId { get; set; }
	public AppUser Sender { get; set; }
	public string Content { get; set; }
	public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
	public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
	public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}