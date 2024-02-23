

namespace API.DTOs;

public record ContactDTO
{
	public int Id { get; init; }
	public string UserName { get; init; }
	public string Email { get; init; }
	public DateTime Created { get; init; }
	public DateTime LastActive { get; init; }
	public string PhotoUrl { get; init; }
	// public PhotoDTO     Photo {get; init;}
}


