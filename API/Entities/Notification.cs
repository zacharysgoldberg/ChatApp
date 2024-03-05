namespace API.Entities;

public class Notification
{
	public int Id { get; set; }
	public int SenderId { get; set; }
	public AppUser Sender { get; set; }
	public int? RecipientId { get; set; }
	public AppUser Recipient { get; set; }
	public int? MessageId { get; set; }
	public Message Message { get; set; }
	public int? GroupMessageId { get; set; }
	public GroupMessage GroupMessage { get; set; }
	public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
	public string Content { get; set; }
}
