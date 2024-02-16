namespace API.DTOs;

public record class CreateMessageDTO
{
    public string RecipientUsername { get; init; }
    public int RecipientId { get; init; }
    public string Content { get; init; }
}
