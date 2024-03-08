using System.Numerics;

namespace API.DTOs
{
	public record CreateChannelDTO
	{
		public Guid? ChannelId { get; init; }
		public string? ChannelName { get; init; }
		public string Content { get; init; }
		public ICollection<int>? ContactIds { get; init; }
	}
}
