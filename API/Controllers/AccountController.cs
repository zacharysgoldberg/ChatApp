using System.Collections;
using System.Security.Cryptography;
using System.Text;

// using API.Controllers;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(ITokenService             tokenService, 
                                 UserManager<AppUser>      userManager)
        {
            _tokenService = tokenService;
            _userManager  = userManager;
        }

        [HttpPost("register")] // POST /api/account/register?username=dave&password=pwd
        public async Task<ActionResult> Register(RegisterDTO registerDto)
        {
            if(await UserExists(registerDto.Email.ToLower()))
                return BadRequest("Email is already in use");

            AppUser user = new AppUser
            {
                Email = registerDto.Email.ToLower(),
                UserName = registerDto.Email.ToLower(),
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                Console.WriteLine("----------------\n");
                foreach(var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
                    
                return BadRequest("User creation failed! Please check user details and try again.");
            }
            
            return Ok("User creation successful!");
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByNameAsync(loginDTO.Username.ToLower());

            if(user != null && await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                return new UserDTO
                {
                    Username = user.UserName,
                    Token = _tokenService.CreateToken(user)
                };
            }

            return Unauthorized("Invalid Username or Password");
        }

        /*
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Cancel or Remove JWT from user 
        
            return NoContent();
        }
        */

        private async Task<bool> UserExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
                return true;
            return false;
        }
    }
}
