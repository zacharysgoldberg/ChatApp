

namespace API.DTOs;

public record ContactDTO
{
    public int          Id {get; init;}
    public string       UserName {get; init;}
    public string       Email {get; init;}
    public DateTime     Created {get; init;} = DateTime.UtcNow;
    public DateTime     LastActive {get; init;} = DateTime.UtcNow;
    public string       PhotoUrl {get; init;}
    public PhotoDTO     Photo {get; init;}
}


