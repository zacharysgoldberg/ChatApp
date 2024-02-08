using System.ComponentModel.DataAnnotations;

public class ChangePasswordDTO
{
    [Required]
    [DataType(DataType.Password)]
    public string  CurrentPassword {get; set;}
    [Required]
    [DataType(DataType.Password)]
    public string   Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string   ConfirmPassword { get; set; }
}