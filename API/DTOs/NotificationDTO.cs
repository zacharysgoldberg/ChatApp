namespace API.DTOs;

public record class NotificationDTO
{
	public int Id { get; init; }
	public int SenderId { get; init; }
	public string SenderUsername { get; set; }
	public int? RecipientId { get; init; }
	public string? RecipientUsername { get; set; }
	public int? MessageId { get; init; }
	public int? GroupMessageId { get; init; }
	public Guid? ChannelId { get; init; }
	public DateTime? CreatedAt { get; init; }
	public string Content { get; init; }
}
