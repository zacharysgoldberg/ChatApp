﻿using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class LoginDTO
{
    [Required(ErrorMessage = "Username/Email is required.")]
    public string Username {get; set;}
    [Required(ErrorMessage = "Password is required.")]
    public string Password {get; set;}
}
