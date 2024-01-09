using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Message
    {
        [Key]
        protected int   MessageId {get; set;}
        public int      MessageFrom {get; set;}
        public int      MessageTo {get; set;}
        public string   Content {get; set;}
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    }
}