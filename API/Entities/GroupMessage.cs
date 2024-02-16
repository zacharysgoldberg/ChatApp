using System.Numerics;

namespace API.Entities;

public class GroupMessage
{
    public int          Id { get; set; }
    public BigInteger   ChannelId { get; set; }
    // public AppUser      Channel { get; set; }
    public int          UserId { get; set; }
    public AppUser      User { get; set; }
    public string       Content { get; set; }
    public DateTime?    CreatedAt { get; set; } = DateTime.UtcNow;
}