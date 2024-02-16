using API.DTOs;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public string                       RefreshToken { get; set; }
        public DateTime                     RefreshTokenExpiryTime { get; set; }
        public DateTime                     Created {get; set;} = DateTime.UtcNow;
        public DateTime                     LastActive {get; set;} = DateTime.UtcNow;
        public Photo                        Photo {get; set;}
        public ICollection<UserContact>     UserContacts { get; set; }
        public ICollection<Message>         MessagesSent { get; set; }
        public ICollection<Message>         MessagesReceived { get; set; }
    }
}

