namespace API.DTOs;

public class UserDTO
{
    public string Username { get; set; }
    
    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }
}
