namespace API.Entities;

public class Message
{
	public int Id { get; set; }
	public int SenderId { get; set; }
	public AppUser Sender { get; set; }
	public int RecipientId { get; set; }
	public AppUser Recipient { get; set; }
	public string Content { get; set; }
	public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
	public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}