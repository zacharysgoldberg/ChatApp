
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class MemberUpdateDTO
{
    public int          Id {get; set;}
    public string       UserName {get; set;}
    public string?      Email {get; set;}
    public string       PhotoUrl {get; set;}
    [DataType(DataType.PhoneNumber)]
    public string?      PhoneNumber {get; set;}
}
