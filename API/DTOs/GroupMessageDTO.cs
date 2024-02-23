using System.Numerics;
using API.Entities;

namespace API.DTOs;

public record GroupMessageDTO
{

	public int Id { get; init; }
	public Guid ChannelId { get; init; }
	public int SenderId { get; set; }
	public string SenderUsername { get; init; }
	public string SenderPhotoUrl { get; init; }
	public string Content { get; init; }
	public DateTime? CreatedAt { get; init; } = DateTime.UtcNow;
	public IEnumerable<ContactDTO> Contacts { get; init; }  // May not need this
}