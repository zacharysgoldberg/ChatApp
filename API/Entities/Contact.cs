using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace API.Entities;

public class Contact
{
	public int Id { get; set; } // Primary key

	[Required]
	public int UserId { get; set; } // Foreign key for the AppUser
	public AppUser AppUser { get; set; } // Navigation property to the AppUser
}