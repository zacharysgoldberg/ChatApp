
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email {get; set;}

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password {get; set;}

        [Required(ErrorMessage = "Password Confirmation is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}