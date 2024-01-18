using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser : IdentityUser<int>
    {
        /*
        [Key]
        public int                      Id { get; set; }
        // [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string                   UserName { get; set; }
        public byte[]                   PasswordHash {get; set;}
        public byte[]                   PasswordSalt { get; set; }
        // public List<string>  Contacts { get; set; } // { Username/Email, ... }
        */

    }

}

