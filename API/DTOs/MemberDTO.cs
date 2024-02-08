
using API.Entities;

namespace API.DTOs;

public class MemberDTO
{
    public int                  Id {get; set;}
    public string               UserName {get; set;}
    public string               Email {get; set;}
    public string?              PhoneNumber {get; set;}
    public string               RefreshToken { get; set; }
    public DateTime             RefreshTokenExpiryTime { get; set; }
    public DateTime             Created {get; set;} = DateTime.UtcNow;
    public DateTime             LastActive {get; set;} = DateTime.UtcNow;
    public string               PhotoUrl {get; set;}
    public PhotoDTO             Photo {get; set;}
    public ICollection<UserContact>     UserContacts {get; set;}

}
