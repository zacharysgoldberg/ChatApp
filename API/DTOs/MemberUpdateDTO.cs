
namespace API.DTOs;

public class MemberUpdateDTO
{
    public int      Id {get; set;}
    public string   UserName {get; set;}
    public string   Email {get; set;}
    public DateTime LastActive {get; set;} = DateTime.UtcNow;
    public string   PhotoUrl {get; set;}
    // public PhotoDTO Photo {get; set;}
    public List<MemberDTO>  Contacts {get; set;} = new();
}
