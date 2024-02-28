namespace API.Entities;

public class Notification
{
	public int Id { get; set; }
	public int SenderId { get; set; }
	public AppUser Sender { get; set; }
	public int? RecipientId { get; set; }
	public AppUser Recipient { get; set; }
	public Guid? ChannelId { get; set; }
	public GroupMessage Channel { get; set; }
}
