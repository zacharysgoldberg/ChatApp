namespace API.DTOs;

public record class CreateMessageDTO
{
	public int RecipientId { get; init; }
	public string Content { get; init; }
}
