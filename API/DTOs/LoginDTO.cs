using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public record LoginDTO
{
    [Required(ErrorMessage = "Username/Email is required.")]
    public required string Username {get; set;}
    [Required(ErrorMessage = "Password is required.")]
    public required string Password {get; set;}
}

