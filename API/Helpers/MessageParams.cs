namespace API.Helpers;

public class MessageParams : PaginationParams
{
    // public string Username { get; set; }
    public int Id { get; set;}
    public string Container {get; set; } = "Undread";
}
