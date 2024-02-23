using API.Entities;

namespace API.DTOs;

public record class MessageDTO
{
	public int Id { get; init; }
	public int SenderId { get; init; }
	public string SenderUsername { get; set; }
	public string SenderPhotoUrl { get; init; }
	public int RecipientId { get; init; }
	public string RecipientUsername { get; set; }
	public string RecipientPhotoUrl { get; init; }
	public string Content { get; init; }
	public DateTime? DateRead { get; init; }
	public DateTime? CreatedAt { get; init; }
}
