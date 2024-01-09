
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class GroupMessage
    {
        [Key]
        public int      ChannelId {get; set;}
        public int      MessageId {get; set;}
        public int      UserId {get; set;}
        public string   Content {get; set;}
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    }
}