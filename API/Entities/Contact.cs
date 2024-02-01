using System.ComponentModel.DataAnnotations.Schema;
using API.DTOs;

namespace API.Entities;

[Table("Contacts")]
public class Contact
{
    public int          Id {get; set;}
    public string       UserName {get; set;}
    public string       Email {get; set;}
    public DateTime     LastActive {get; set;} = DateTime.UtcNow;
    public PhotoDTO     Photo {get; set;}
    public int          AppUserId {get; set;}
    public AppUser      AppUser {get; set;}
    
}