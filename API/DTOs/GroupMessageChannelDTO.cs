namespace API.DTOs;

public record GroupMessageChannelDTO
{
	public Guid ChannelId { get; init; }
	public IEnumerable<ContactDTO> Contacts { get; init; }
}
