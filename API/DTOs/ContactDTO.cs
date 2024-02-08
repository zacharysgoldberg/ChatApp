

namespace API.DTOs;

public class ContactDTO
{
    public int          Id {get; set;}
    public string       UserName {get; set;}
    public string       Email {get; set;}
    public DateTime     Created {get; set;} = DateTime.UtcNow;
    public DateTime     LastActive {get; set;} = DateTime.UtcNow;
    public string       PhotoUrl {get; set;}
    public PhotoDTO     Photo {get; set;}

}
