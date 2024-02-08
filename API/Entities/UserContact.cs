using API.Entities;

public class UserContact
    {
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public int ContactId { get; set; }
        public Contact Contact { get; set; }
    }