namespace API.DTOs;

public record class NotificationDTO
{
	public int Id { get; init; }
	public int SenderId { get; init; }
	public string? SenderUsername { get; set; }
	public int? ReceipientId { get; init; }
	public string? RecipientUsername { get; set; }
	public Guid? ChannelId { get; init; }
	public string? ChannelName { get; set; }
}
