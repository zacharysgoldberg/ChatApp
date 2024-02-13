namespace API.DTOs;

public record PhotoDTO
{
    public required int      Id { get; init; }
    public required string   Url { get; init; }
}
