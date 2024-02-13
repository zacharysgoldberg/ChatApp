
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public record MemberUpdateDTO
{
    public int          Id {get; init;}
    public string       UserName {get; init;}
    public string?      Email {get; init;}
    [DataType(DataType.PhoneNumber)]
    public string?      PhoneNumber {get; init;}
}
