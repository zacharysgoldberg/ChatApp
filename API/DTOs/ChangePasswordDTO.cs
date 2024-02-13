using System.ComponentModel.DataAnnotations;

public record ChangePasswordDTO
{
    [Required]
    [DataType(DataType.Password)]
    public string  CurrentPassword {get; init;}
    [Required]
    [DataType(DataType.Password)]
    public string   Password { get; init; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string   ConfirmPassword { get; init; }
}