using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace API.Entities
{
    public class AppUser
    {
        [Key]
        public int                      Id {get; set;}
        [Required]
        public string                   UserName {get; set;}
        public byte[]                   PasswordHash {get; set;}
        public byte[]                   PasswordSalt { get; set; }
        // public Dictionary<string, int> Contacts {get; set;}

    }

}

