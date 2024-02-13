namespace API.DTOs;

public record UserDTO
{
    public string   Username { get; init; }
    
    public string?  AccessToken { get; init; }

    public string?  RefreshToken { get; init; }
}
